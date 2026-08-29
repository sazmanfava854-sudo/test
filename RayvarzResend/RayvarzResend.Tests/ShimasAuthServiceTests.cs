using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class ShimasAuthServiceTests
{
    private static ShimasAuthService CreateService(
        ShimasAuthOptions? options = null,
        InMemoryAppUserStore? memory = null)
    {
        memory ??= new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(
            config,
            memory,
            NullLogger<AppUserRepository>.Instance);

        return new ShimasAuthService(
            Options.Create(options ?? new ShimasAuthOptions()),
            repo,
            new TestHttpClientFactory(),
            NullLogger<ShimasAuthService>.Instance);
    }

    [Fact]
    public void BuildExternalLoginUrl_includes_lkey_and_returnUrl()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            LKey = "test-lkey-123",
            LoginUrl = "https://login.mashhad.ir/Authentication/Login.aspx"
        });

        var url = service.BuildExternalLoginUrl("https://app.example.com/auth/callback");

        Assert.Contains("lkey=test-lkey-123", url);
        Assert.Contains("returnUrl=", url);
        Assert.Contains(Uri.EscapeDataString("https://app.example.com/auth/callback"), url);
    }

    [Fact]
    public void BuildExternalLoginUrl_throws_when_lkey_missing()
    {
        var service = CreateService(new ShimasAuthOptions { Enabled = true, LKey = "" });
        Assert.Throws<InvalidOperationException>(() =>
            service.BuildExternalLoginUrl("https://app.example.com/auth/callback"));
    }

    [Fact]
    public void ResolveLoginRedirectPath_prefers_sso_when_ready()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            LKey = "abc",
            AllowLocalLoginFallback = true
        });

        Assert.Equal("/auth/login", service.ResolveLoginRedirectPath());
    }

    [Fact]
    public void ResolveLoginRedirectPath_uses_local_when_sso_disabled()
    {
        var service = CreateService(new ShimasAuthOptions { Enabled = false });
        Assert.Equal("/login.html", service.ResolveLoginRedirectPath());
    }

    [Fact]
    public void GetStatus_reflects_options()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            LKey = "key",
            AllowLocalLoginFallback = false
        });

        var status = service.GetStatus();

        Assert.True(status.Enabled);
        Assert.True(status.SsoReady);
        Assert.True(status.PreferSsoLogin);
        Assert.False(status.LocalLoginAvailable);
        Assert.Equal("/auth/login", status.LoginPath);
        Assert.Equal("/auth/callback", status.CallbackPath);
    }

    [Fact]
    public async Task ValidateAsync_succeeds_with_stub_when_remote_url_missing()
    {
        var service = CreateService(new ShimasAuthOptions { Enabled = true, ValidateTokenUrl = "" });

        var result = await service.ValidateAsync("1234567890", "refresh-token-abc");

        Assert.True(result.Success);
        Assert.False(result.UsedRemoteApi);
        Assert.Equal("1234567890", result.Profile.Username);
    }

    [Fact]
    public async Task ValidateAsync_rejects_empty_username()
    {
        var service = CreateService();
        var result = await service.ValidateAsync("", "token");
        Assert.False(result.Success);
        Assert.Contains("username", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_rejects_short_refresh_token()
    {
        var service = CreateService();
        var result = await service.ValidateAsync("user1", "ab");
        Assert.False(result.Success);
        Assert.Contains("refresh_token", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResolveOrCreateUserAsync_auto_provisions_new_user()
    {
        var service = CreateService(new ShimasAuthOptions { AutoProvisionUsers = true });

        var user = await service.ResolveOrCreateUserAsync(new ShimasUserProfile
        {
            Username = "9988776655",
            FirstName = "علی",
            LastName = "رضایی"
        });

        Assert.NotNull(user);
        Assert.Equal("9988776655", user!.Username);
        Assert.Equal("علی", user.FirstName);
        Assert.Equal("رضایی", user.LastName);
        Assert.True(user.IsActive);
        Assert.False(user.IsAdmin);
    }

    [Fact]
    public async Task ResolveOrCreateUserAsync_returns_existing_user()
    {
        var memory = new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        var existing = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "1122334455",
            Password = "Secret@123",
            FirstName = "موجود",
            LastName = "کاربر",
            NationalId = "1122334455",
            District = "1"
        });

        var service = CreateService(new ShimasAuthOptions { AutoProvisionUsers = true }, memory);
        var user = await service.ResolveOrCreateUserAsync(new ShimasUserProfile { Username = "1122334455" });

        Assert.NotNull(user);
        Assert.Equal(existing.Id, user!.Id);
    }

    [Fact]
    public void BuildCallbackAbsoluteUrl_uses_request_host()
    {
        var service = CreateService();
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("app.example.com");

        var callback = service.BuildCallbackAbsoluteUrl(context.Request);

        Assert.Equal("https://app.example.com/auth/callback", callback);
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}

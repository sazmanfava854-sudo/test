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
        Assert.Contains("client_id=test-lkey-123", url);
        Assert.DoesNotContain("secret=", url, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("D2fbf", url);
        Assert.Contains("returnUrl=", url);
        Assert.Contains(Uri.EscapeDataString("https://app.example.com/auth/callback"), url);
    }

    [Fact]
    public void BuildExternalLoginUrl_uses_ClientId_when_LKey_empty()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf",
            LKey = "",
            LoginUrl = "https://login.mashhad.ir/Authentication/Login.aspx"
        });

        var url = service.BuildExternalLoginUrl("https://city.mashhad.ir:5065/auth/callback");

        Assert.Contains("lkey=19cf3C33", url);
        Assert.Contains("client_id=19cf3C33", url);
        Assert.DoesNotContain("D2fbf", url);
        Assert.Contains(Uri.EscapeDataString("https://city.mashhad.ir:5065/auth/callback"), url);
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
    public void Localhost_keeps_local_login_while_public_host_uses_sso()
    {
        var options = new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf",
            AllowLocalLoginFallback = true,
            PublicBaseUrl = "https://city.mashhad.ir:5065"
        };

        Assert.True(ShimasAuthOptions.IsLoopbackHost("localhost"));
        Assert.True(ShimasAuthOptions.IsLoopbackHost("localhost:5000"));
        Assert.True(ShimasAuthOptions.IsLoopbackHost("127.0.0.1"));
        Assert.False(ShimasAuthOptions.IsLoopbackHost("city.mashhad.ir"));

        Assert.False(options.PreferSsoLoginForHost("localhost"));
        Assert.True(options.LocalLoginAvailableForHost("localhost"));
        Assert.True(options.PreferSsoLoginForHost("city.mashhad.ir"));
        Assert.False(options.LocalLoginAvailableForHost("city.mashhad.ir"));

        var service = CreateService(options);
        var local = new DefaultHttpContext();
        local.Request.Host = new HostString("localhost", 5000);
        Assert.Equal("/login.html", service.ResolveLoginRedirectPath(local.Request));
        Assert.False(service.GetStatus(local.Request).PreferSsoLogin);
        Assert.True(service.GetStatus(local.Request).LocalLoginAvailable);

        var server = new DefaultHttpContext();
        server.Request.Host = new HostString("city.mashhad.ir", 5065);
        Assert.Equal("/auth/login", service.ResolveLoginRedirectPath(server.Request));
        Assert.True(service.GetStatus(server.Request).PreferSsoLogin);
    }

    [Fact]
    public async Task Existing_admin_without_domain_gets_bootstrap_domain()
    {
        var memory = new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "admin",
            Password = "Admin@1234",
            FirstName = "مدیر",
            LastName = "سیستم",
            NationalId = "1234567890",
            Domain = "placeholder",
            IsAdmin = true
        });
        memory.FindByUsername("admin")!.Domain = "";

        Assert.True(await repo.EnsureAdminDomainIfEmptyAsync("admin", "admin"));
        Assert.Equal("admin", memory.FindByUsername("admin")!.Domain);
        Assert.False(await repo.EnsureAdminDomainIfEmptyAsync("admin", "other-domain"));
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
            Domain = "hoseine-sh",
            District = "1"
        });

        var service = CreateService(new ShimasAuthOptions { AutoProvisionUsers = true }, memory);
        var user = await service.ResolveOrCreateUserAsync(new ShimasUserProfile { Username = @"MASHHAD\hoseine-sh" });

        Assert.NotNull(user);
        Assert.Equal(existing.Id, user!.Id);
    }

    [Fact]
    public async Task ResolveOrCreateUserAsync_matches_domain_field()
    {
        var memory = new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        var existing = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "0011223344",
            Password = "Secret@123",
            FirstName = "حسین",
            LastName = "حسینی",
            NationalId = "0011223344",
            Domain = "hoseine-sh",
            District = "1"
        });

        var service = CreateService(new ShimasAuthOptions { AutoProvisionUsers = false }, memory);
        var user = await service.ResolveOrCreateUserAsync(new ShimasUserProfile { Username = "hoseine-sh" });

        Assert.NotNull(user);
        Assert.Equal(existing.Id, user!.Id);
        Assert.Equal("hoseine-sh", user.Domain);
    }

    [Fact]
    public void ParseCallbackQuery_strips_windows_domain_prefix()
    {
        var service = CreateService();
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?userName=MASHHAD%5Choseine-sh&refreshToken=abc-token-xyz");

        var payload = service.ParseCallbackQuery(context.Request.Query);

        Assert.Equal("hoseine-sh", payload.Username);
        Assert.Equal("abc-token-xyz", payload.RefreshToken);
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

    [Fact]
    public void BuildCallbackAbsoluteUrl_uses_public_base_url_when_configured()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            PublicBaseUrl = "http://5.252.216.140:8070"
        });
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost:5000");

        var callback = service.BuildCallbackAbsoluteUrl(context.Request);

        Assert.Equal("http://5.252.216.140:8070/auth/callback", callback);
    }

    [Fact]
    public void BuildCallbackAbsoluteUrl_uses_city_mashhad_public_base()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            PublicBaseUrl = "https://city.mashhad.ir:5065"
        });
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost:5000");

        var callback = service.BuildCallbackAbsoluteUrl(context.Request);

        Assert.Equal("https://city.mashhad.ir:5065/auth/callback", callback);
        Assert.DoesNotContain("login.html", callback);
    }

    [Fact]
    public void ParseCallbackQuery_reads_username_and_refresh_token_aliases()
    {
        var service = CreateService();
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?userName=1234567890&refreshToken=abc-token-xyz");

        var payload = service.ParseCallbackQuery(context.Request.Query);

        Assert.Equal("1234567890", payload.Username);
        Assert.Equal("abc-token-xyz", payload.RefreshToken);
    }

    [Fact]
    public void GetStatus_includes_registered_callback_from_public_base_url()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            LKey = "key",
            PublicBaseUrl = "http://5.252.216.140:8070"
        });

        var status = service.GetStatus();

        Assert.Equal("http://5.252.216.140:8070/auth/callback", status.RegisteredCallbackUrl);
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class ShimasAuthServiceTests
{
    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = "";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    internal static IHostEnvironment StubHost(string environmentName = "Production") =>
        new StubHostEnvironment { EnvironmentName = environmentName };

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

        var opts = Options.Create(options ?? new ShimasAuthOptions());
        var httpFactory = new TestHttpClientFactory();
        var mashhad = new MashhadSsoApiClient(
            opts,
            httpFactory,
            NullLogger<MashhadSsoApiClient>.Instance);

        return new ShimasAuthService(
            opts,
            repo,
            mashhad,
            httpFactory,
            NullLogger<ShimasAuthService>.Instance,
            StubHost());
    }

    [Fact]
    public void BuildLoginStartUrl_uses_official_start_path()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            LoginStartUrlTemplate = "https://login.mashhad.ir/Authentication/Start/{loginKey}"
        });

        var url = service.BuildLoginStartUrl("a1b2-GUID-c3d4");
        Assert.Equal("https://login.mashhad.ir/Authentication/Start/a1b2-GUID-c3d4", url);
    }

    [Fact]
    public async Task BuildExternalLoginUrlAsync_skips_loginKey_when_disabled()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "19cf3C33",
            ClientSecret = "secret",
            ApiBaseUrl = "https://login.mashhad.ir",
            ApiName = "FinancialAssistant",
            UseLoginKeyOnRedirect = false,
            LoginUrl = "https://login.mashhad.ir/Authentication/Login.aspx"
        });

        var url = await service.BuildExternalLoginUrlAsync("https://city.mashhad.ir:5065/auth/callback");
        Assert.Contains("lkey=19cf3C33", url);
        Assert.Contains("returnUrl=", url);
        Assert.DoesNotContain("loginKey=", url);
    }

    [Fact]
    public void BuildExternalLoginUrl_sends_return_url_only_once_case_insensitively()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "19cf3C33",
            LoginUrl = "https://login.mashhad.ir/Authentication/Login.aspx"
        });

        var url = service.BuildExternalLoginUrl("https://city.mashhad.ir:5065/management");
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(url).Query);
        var returnKeys = query.Keys.Where(k => k.Equals("returnUrl", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.Single(returnKeys);
        Assert.Equal("https://city.mashhad.ir:5065/management", query[returnKeys[0]].ToString());
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
        options.AllowSsoOnLoopbackForDebug = true;
        Assert.True(options.PreferSsoLoginForHost("localhost"));
        options.AllowSsoOnLoopbackForDebug = false;
        Assert.True(options.LocalLoginAvailableForHost("localhost"));
        Assert.True(options.PreferSsoLoginForHost("city.mashhad.ir"));
        Assert.False(options.LocalLoginAvailableForHost("city.mashhad.ir"));

        var service = CreateService(options);
        var local = new DefaultHttpContext();
        local.Request.Host = new HostString("localhost", 5000);
        Assert.Equal("https://city.mashhad.ir:5065/auth/login", service.ResolveLoginRedirectPath(local.Request));
        Assert.False(service.GetStatus(local.Request).PreferSsoLogin);
        Assert.True(service.GetStatus(local.Request).LocalLoginAvailable);
        Assert.Equal("https://city.mashhad.ir:5065/auth/login", service.GetStatus(local.Request).PublicSsoLoginUrl);
        Assert.True(service.UsesPublicSsoLoginUrl(local.Request));

        options.AllowSsoOnLoopbackForDebug = true;
        var serviceDebug = CreateService(options);
        Assert.Equal("https://city.mashhad.ir:5065/auth/login", serviceDebug.ResolveLoginRedirectPath(local.Request));
        Assert.True(serviceDebug.GetStatus(local.Request).AllowSsoOnLoopbackForDebug);

        var server = new DefaultHttpContext();
        server.Request.Host = new HostString("city.mashhad.ir", 5065);
        Assert.Equal("/auth/login", service.ResolveLoginRedirectPath(server.Request));
        Assert.True(service.GetStatus(server.Request).PreferSsoLogin);
        Assert.True(service.GetStatus(server.Request).AllowAdminLocalLoginOnPublicHost);
    }

    [Fact]
    public void Internal_server_host_uses_login_html_without_sso_redirect()
    {
        var options = new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf",
            AllowLocalLoginFallback = true,
            PreferLocalLoginHosts = new[] { "5.252.216.140" },
            PublicBaseUrl = "https://city.mashhad.ir:5065",
            PostLoginDefaultPath = "/management/"
        };

        Assert.True(options.IsPreferLocalLoginHost("5.252.216.140"));
        Assert.True(options.IsPreferLocalLoginHost("5.252.216.140:8070"));
        Assert.False(options.PreferSsoLoginForHost("5.252.216.140:8070"));
        Assert.True(options.LocalLoginAvailableForHost("5.252.216.140:8070"));

        var service = CreateService(options);
        var internalReq = new DefaultHttpContext();
        internalReq.Request.Host = new HostString("5.252.216.140", 8070);
        Assert.Equal("/login.html", service.ResolveLoginRedirectPath(internalReq.Request));
        Assert.False(service.GetStatus(internalReq.Request).PreferSsoLogin);
        Assert.True(service.GetStatus(internalReq.Request).LocalLoginAvailable);
        Assert.Equal("/management/", service.GetStatus(internalReq.Request).PostLoginDefaultPath);
    }

    [Fact]
    public void GetStatus_exposes_admin_local_login_flag()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "id",
            AllowAdminLocalLoginOnPublicHost = true
        });
        Assert.True(service.GetStatus().AllowAdminLocalLoginOnPublicHost);
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
    public async Task User_with_two_comma_separated_domains_resolves_from_either()
    {
        var memory = new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "0925569917",
            Password = "Pass@1234",
            FirstName = "شقایق",
            LastName = "حسینی",
            NationalId = "0925569917",
            Position = "کارشناس",
            District = "7",
            Domain = "hoseine-sh, sadathoseini-sh"
        });
        Assert.Equal("hoseine-sh,sadathoseini-sh", user.Domain);

        Assert.Equal(user.Id, (await repo.FindBySsoIdentityAsync("hoseine-sh"))?.Id);
        Assert.Equal(user.Id, (await repo.FindBySsoIdentityAsync("sadathoseini-sh"))?.Id);
        Assert.Equal(user.Id, (await repo.FindBySsoIdentityAsync(@"MASHHAD\sadathoseini-sh"))?.Id);
        Assert.Null(await repo.FindBySsoIdentityAsync("hoseine"));

        var service = CreateService(new ShimasAuthOptions { AutoProvisionUsers = false }, memory);
        foreach (var domain in new[] { "hoseine-sh", "sadathoseini-sh" })
        {
            var resolved = await service.ResolveOrCreateUserAsync(new ShimasUserProfile { Username = domain, Domain = domain });
            Assert.Equal(user.Id, resolved?.Id);
        }
    }

    [Theory]
    [InlineData("hoseine-sh,sadathoseini-sh", "hoseine-sh,sadathoseini-sh", true)]
    [InlineData(" hoseine-sh ; MASHHAD\\sadathoseini-sh ", "hoseine-sh,sadathoseini-sh", true)]
    [InlineData("hoseine-sh", "hoseine-sh", true)]
    [InlineData("hoseine-sh,bad name", "hoseine-sh,bad name", false)]
    [InlineData(",", "", false)]
    public void Domain_list_normalizes_and_validates(string input, string normalized, bool valid)
    {
        Assert.Equal(normalized, AppUserDomainNormalizer.NormalizeList(input));
        Assert.Equal(valid, AppUserDomainNormalizer.IsValidList(input));
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
    public void BuildCallbackAbsoluteUrl_uses_sso_registered_return_url_when_set()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            SsoRegisteredReturnUrl = "https://city.mashhad.ir:5065",
            PublicBaseUrl = "https://wrong.example.com",
            CallbackPath = "/"
        });
        var context = new DefaultHttpContext();
        Assert.Equal("https://city.mashhad.ir:5065", service.BuildCallbackAbsoluteUrl(context.Request));
    }

    [Fact]
    public void BuildCallbackAbsoluteUrl_uses_public_base_only_when_callback_path_is_root()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            PublicBaseUrl = "https://city.mashhad.ir:5065",
            CallbackPath = "/"
        });
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("city.mashhad.ir", 5065);

        var callback = service.BuildCallbackAbsoluteUrl(context.Request);
        var status = service.GetStatus(context.Request);

        Assert.Equal("https://city.mashhad.ir:5065", callback);
        Assert.Equal("https://city.mashhad.ir:5065", status.RegisteredCallbackUrl);
        Assert.Equal("/", status.CallbackPath);
    }

    [Fact]
    public void IsSsoCallbackHttpRequest_detects_root_return_with_token_query()
    {
        var service = CreateService(new ShimasAuthOptions { CallbackPath = "/" });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/";
        context.Request.QueryString = new QueryString("?userName=1234567890&refreshToken=abc-token-xyz");

        Assert.True(service.IsSsoCallbackHttpRequest(context.Request));

        context.Request.QueryString = QueryString.Empty;
        Assert.False(service.IsSsoCallbackHttpRequest(context.Request));
    }

    [Fact]
    public void BuildCallbackAbsoluteUrl_uses_management_return_url_when_registered()
    {
        var service = CreateService(new ShimasAuthOptions
        {
            SsoRegisteredReturnUrl = "https://city.mashhad.ir:5065/management",
            CallbackPath = "/management"
        });
        var context = new DefaultHttpContext();
        Assert.Equal("https://city.mashhad.ir:5065/management", service.BuildCallbackAbsoluteUrl(context.Request));
    }

    [Theory]
    [InlineData("/management")]
    [InlineData("/management/")]
    [InlineData("/MANAGEMENT")]
    [InlineData("/MANAGMENT")]
    [InlineData("/MANAGMENT/index.html")]
    public void IsSsoCallbackHttpRequest_detects_management_path_with_token_query(string path)
    {
        var service = CreateService(new ShimasAuthOptions { CallbackPath = "/management" });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = path;
        context.Request.QueryString = new QueryString("?userName=1234567890&refreshToken=abc-token-xyz");

        Assert.True(service.IsSsoCallbackHttpRequest(context.Request));
    }

    [Fact]
    public void ResolvePostLoginRedirect_falls_back_to_management_hub_without_cookie()
    {
        var service = CreateService(new ShimasAuthOptions { PostLoginDefaultPath = "/management/" });
        var context = new DefaultHttpContext();
        Assert.Equal("/management/", service.ResolvePostLoginRedirect(context));
    }

    [Theory]
    [InlineData("/management", true)]
    [InlineData("/MANAGMENT", true)]
    [InlineData("/MANAGMENT/management.js", true)]
    [InlineData("/management/", false)]
    [InlineData("/management/management.js", false)]
    [InlineData("/index.html", false)]
    public void ManagementHubPaths_redirects_non_canonical_paths(string path, bool expected)
    {
        Assert.Equal(expected, ManagementHubPaths.NeedsCanonicalRedirect(path));
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
    public void ProbeSsoCallbackHttpRequest_explains_missing_username_on_management()
    {
        var service = CreateService(new ShimasAuthOptions { CallbackPath = "/management" });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/management";
        context.Request.QueryString = new QueryString("?refresh_token=only-token-value-here");

        var probe = service.ProbeSsoCallbackHttpRequest(context.Request);

        Assert.False(probe.IsSsoCallbackHttpRequest);
        Assert.True(probe.PathMatchesCallback);
        Assert.Contains("username", probe.RejectionReasonFa ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsSsoCallbackHttpRequest_matches_path_with_path_base()
    {
        var service = CreateService(new ShimasAuthOptions { CallbackPath = "/management" });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.PathBase = "/app";
        context.Request.Path = "/management";
        context.Request.QueryString = new QueryString("?userName=1234567890&refreshToken=abc-token-xyz");

        Assert.True(service.IsSsoCallbackHttpRequest(context.Request));
        Assert.Equal("/app/management", service.ResolveRequestPathForCallback(context.Request));
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

    [Fact]
    public void ValidateReturnedState_allows_legacy_callback_without_state_or_cookie()
    {
        var service = CreateService(new ShimasAuthOptions { LoginState = "test" });
        var context = new DefaultHttpContext();

        Assert.True(service.ValidateReturnedState(context, null));
        Assert.True(service.ValidateReturnedState(context, ""));
    }

    [Fact]
    public void ValidateReturnedState_requires_cookie_match_for_loginKey_flow()
    {
        var service = CreateService(new ShimasAuthOptions { LoginState = "test" });
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{ShimasAuthService.SsoStateCookieName}=abc-state";

        Assert.True(service.ValidateReturnedState(context, "abc-state"));
        Assert.False(service.ValidateReturnedState(context, "other"));
    }

    [Fact]
    public void ResolvePostLoginRedirect_uses_safe_relative_path_only()
    {
        var service = CreateService();
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{ShimasAuthService.PostLoginReturnCookieName}=%2F";

        Assert.Equal("/", service.ResolvePostLoginRedirect(context));

        context.Request.Headers.Cookie = $"{ShimasAuthService.PostLoginReturnCookieName}=https%3A%2F%2Fevil";
        Assert.Equal("/", service.ResolvePostLoginRedirect(context));
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}

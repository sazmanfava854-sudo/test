using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class MashhadSsoApiClientTests
{
    [Fact]
    public async Task GetLoginKey_omits_ReturnUrl_when_RuleEngine_style()
    {
        string? loginKeyBody = null;
        var handler = new MashhadSsoFakeHandler(b => loginKeyBody = b);
        var httpFactory = new NamedHttpClientFactory(handler);
        var options = Options.Create(new ShimasAuthOptions
        {
            ApiBaseUrl = "https://login.mashhad.ir",
            ApiName = "zavabetapp",
            ClientId = "4d7475D499c02B3",
            ClientSecret = "51377AC",
            IncludeReturnUrlInLoginKey = false,
            LoginState = "test"
        });
        var api = new MashhadSsoApiClient(options, httpFactory, NullLogger<MashhadSsoApiClient>.Instance);

        var result = await api.GetLoginKeyAsync("https://city.mashhad.ir:5065", "test");
        Assert.True(result.IsSuccess, $"ErrorCode={result.ErrorCode} msg={result.ErrorMessage}");
        Assert.NotNull(loginKeyBody);
        Assert.Contains("\"State\":\"test\"", loginKeyBody.Replace(" ", ""));
        Assert.DoesNotContain("ReturnUrl", loginKeyBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sha256Hex_matches_secret_plus_time_pattern()
    {
        var hash = SsoApiSecretHash.ComputeApiSecret("secret", "12345", "lower");
        Assert.Equal(64, hash.Length);
        Assert.DoesNotContain("-", hash);
    }

    [Fact]
    public async Task GetAccessToken_and_GetUserInfo_flow_maps_profile()
    {
        var handler = new MashhadSsoFakeHandler();
        var httpFactory = new NamedHttpClientFactory(handler);
        var options = Options.Create(new ShimasAuthOptions
        {
            ApiBaseUrl = "https://login.mashhad.ir",
            ApiName = "FinancialAssistant",
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf"
        });
        var api = new MashhadSsoApiClient(options, httpFactory, NullLogger<MashhadSsoApiClient>.Instance);
        var opts = Options.Create(new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf",
            ApiBaseUrl = "https://login.mashhad.ir",
            ApiName = "FinancialAssistant",
            AutoProvisionUsers = false
        });
        var memory = new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "1234567890",
            Password = "Secret@123",
            FirstName = "تست",
            LastName = "کاربر",
            NationalId = "1234567890",
            Domain = "hoseine-sh",
            District = "1"
        });

        var mashhad = new MashhadSsoApiClient(opts, httpFactory, NullLogger<MashhadSsoApiClient>.Instance);
        var service = new ShimasAuthService(
            opts,
            repo,
            mashhad,
            httpFactory,
            NullLogger<ShimasAuthService>.Instance);

        var validation = await service.ValidateAsync("1234567890", "refresh-from-callback");
        Assert.True(validation.Success);
        Assert.True(validation.UsedRemoteApi);
        Assert.Equal("1234567890", validation.Profile.NationalId);
        Assert.Equal("hoseine-sh", validation.Profile.Domain);

        var user = await service.ResolveOrCreateUserAsync(validation.Profile);
        Assert.NotNull(user);
        Assert.Equal("1234567890", user!.Username);
    }

    [Fact]
    public async Task DiagnoseLoginKey_reports_ok_and_loginKey_flow_redirects_to_Start_url()
    {
        var handler = new MashhadSsoFakeHandler();
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var diag = await service.DiagnoseLoginKeyAsync("https://city.mashhad.ir:5065/management");
        Assert.True(diag.GetCurrentTimeOk);
        Assert.True(diag.LoginKeyOk);
        Assert.Null(diag.Error);
        Assert.StartsWith("OK", diag.Verdict);

        var url = await service.BuildExternalLoginUrlAsync("https://city.mashhad.ir:5065/management");
        Assert.Equal("https://login.mashhad.ir/Authentication/Start/lk-123", url);
    }

    [Fact]
    public async Task DiagnoseLoginKey_explains_403_client_info_mismatch_and_falls_back_to_legacy()
    {
        var handler = new MashhadSsoFakeHandler(loginKeyJson: """{"ErrorCode":403,"ErrorMessage":"Client info missmatched","Data":null}""");
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var diag = await service.DiagnoseLoginKeyAsync("https://city.mashhad.ir:5065/management");
        Assert.True(diag.GetCurrentTimeOk);
        Assert.False(diag.LoginKeyOk);
        Assert.Equal(403, diag.LoginKeyErrorCode);
        Assert.Contains("Client info mismatch", diag.Verdict);

        var url = await service.BuildExternalLoginUrlAsync("https://city.mashhad.ir:5065/management");
        Assert.Contains("/Authentication/Login.aspx?", url);
        Assert.Contains("returnUrl=https%3A%2F%2Fcity.mashhad.ir%3A5065%2Fmanagement", url);
    }

    private static ShimasAuthService CreateServiceWithSso(HttpMessageHandler handler, bool useLoginKey)
    {
        var httpFactory = new NamedHttpClientFactory(handler);
        var opts = Options.Create(new ShimasAuthOptions
        {
            Enabled = true,
            ClientId = "53db42619cf3C333b13a18D34fbd9111",
            ClientSecret = "full-secret-from-portal",
            ApiBaseUrl = "https://login.mashhad.ir",
            ApiName = "FinancialAssistant",
            UseLoginKeyOnRedirect = useLoginKey,
            AllowLegacyLoginUrlWithoutLoginKey = true,
            LoginUrl = "https://login.mashhad.ir/Authentication/Login.aspx",
            LoginStartUrlTemplate = "https://login.mashhad.ir/Authentication/Start/{loginKey}"
        });
        var memory = new InMemoryAppUserStore();
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        var mashhad = new MashhadSsoApiClient(opts, httpFactory, NullLogger<MashhadSsoApiClient>.Instance);
        return new ShimasAuthService(opts, repo, mashhad, httpFactory, NullLogger<ShimasAuthService>.Instance);
    }

    private sealed class NamedHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public NamedHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed class MashhadSsoFakeHandler : HttpMessageHandler
    {
        private readonly Action<string?>? _onBody;
        private readonly string _loginKeyJson;

        public MashhadSsoFakeHandler(Action<string?>? onBody = null, string? loginKeyJson = null)
        {
            _onBody = onBody;
            _loginKeyJson = loginKeyJson ?? """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk-123"}}""";
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null)
            {
                var body = await request.Content.ReadAsStringAsync(cancellationToken);
                _onBody?.Invoke(body);
            }

            var path = request.RequestUri?.AbsolutePath ?? "";
            string json;
            if (path.EndsWith("/getCurrentTime", StringComparison.OrdinalIgnoreCase))
            {
                json = """{"ErrorCode":0,"ErrorMessage":"","Data":"1700000000"}""";
            }
            else if (path.EndsWith("/getAccessToken", StringComparison.OrdinalIgnoreCase))
            {
                json = """{"ErrorCode":0,"ErrorMessage":"","Data":{"Status":"ok","AccessToken":"access-token-xyz"}}""";
            }
            else if (path.EndsWith("/getUserInfo", StringComparison.OrdinalIgnoreCase))
            {
                json = """
                {
                  "ErrorCode":0,
                  "ErrorMessage":"",
                  "Data":{
                    "FName":"علی",
                    "LName":"رضایی",
                    "NationalCode":"1234567890",
                    "UserName":"1234567890",
                    "OldSSO_UserInfo":{
                      "basicInfo":{"username":"hoseine-sh","nationalCode":"1234567890"}
                    }
                  }
                }
                """;
            }
            else if (path.EndsWith("/loginKey", StringComparison.OrdinalIgnoreCase))
            {
                json = _loginKeyJson;
            }
            else
            {
                json = """{"ErrorCode":-1,"ErrorMessage":"unknown path"}""";
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}

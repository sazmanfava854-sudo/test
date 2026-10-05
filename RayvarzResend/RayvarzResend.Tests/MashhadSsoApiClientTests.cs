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
    public async Task GetLoginKey_retries_once_when_requestTime_expired()
    {
        var loginKeyCalls = 0;
        var handler = new MashhadSsoFakeHandler(loginKeyResponder: _ =>
        {
            loginKeyCalls++;
            return loginKeyCalls == 1
                ? """{"ErrorCode":401,"ErrorMessage":"requestTime expired","Data":null}"""
                : """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk-retry"}}""";
        });
        var api = new MashhadSsoApiClient(
            Options.Create(new SsoAuthOptions
            {
                ApiBaseUrl = "https://login.mashhad.ir",
                ApiName = "api",
                ClientId = "cid",
                ClientSecret = "sec"
            }),
            new NamedHttpClientFactory(handler),
            NullLogger<MashhadSsoApiClient>.Instance);

        var result = await api.GetLoginKeyAsync(null, "test");
        Assert.True(result.IsSuccess);
        Assert.Equal(2, loginKeyCalls);
    }

    [Fact]
    public async Task GetLoginKey_body_Hash_matches_header_apiSecret()
    {
        string? body = null;
        string? apiSecret = null;
        var handler = new MashhadSsoFakeHandler(
            onBody: b => body = b,
            loginKeyResponder: req =>
            {
                apiSecret = req.Headers.TryGetValues("apiSecret", out var v) ? v.FirstOrDefault() : null;
                return """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk"}}""";
            });
        var api = new MashhadSsoApiClient(
            Options.Create(new SsoAuthOptions
            {
                ApiBaseUrl = "https://login.mashhad.ir",
                ApiName = "api",
                ClientId = "cid",
                ClientSecret = "full-secret-from-portal"
            }),
            new NamedHttpClientFactory(handler),
            NullLogger<MashhadSsoApiClient>.Instance);

        await api.GetLoginKeyAsync(null, "test");
        Assert.NotNull(body);
        Assert.NotNull(apiSecret);
        Assert.Contains($"\"Hash\":\"{apiSecret}\"", body.Replace(" ", ""));
        Assert.Contains("\"Time\":\"1700000000\"", body.Replace(" ", ""));
    }

    [Fact]
    public async Task GetLoginKey_omits_ReturnUrl_when_RuleEngine_style()
    {
        string? loginKeyBody = null;
        var handler = new MashhadSsoFakeHandler(b => loginKeyBody = b);
        var httpFactory = new NamedHttpClientFactory(handler);
        var options = Options.Create(new SsoAuthOptions
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
        Assert.Contains("\"ClientId\":\"4d7475D499c02B3\"", loginKeyBody.Replace(" ", ""));
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
        var options = Options.Create(new SsoAuthOptions
        {
            ApiBaseUrl = "https://login.mashhad.ir",
            ApiName = "FinancialAssistant",
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf"
        });
        var api = new MashhadSsoApiClient(options, httpFactory, NullLogger<MashhadSsoApiClient>.Instance);
        var opts = Options.Create(new SsoAuthOptions
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
        var service = new SsoAuthService(
            opts,
            repo,
            mashhad,
            httpFactory,
            NullLogger<SsoAuthService>.Instance,
            SsoAuthServiceTests.StubHost());

        var validation = await service.ValidateAsync("1234567890", "refresh-from-callback");
        Assert.True(validation.Success);
        Assert.True(validation.UsedRemoteApi);
        Assert.Equal("1234567890", validation.Profile.NationalId);
        Assert.Equal("hoseine-sh", validation.Profile.Domain);
        Assert.Equal("hoseine-sh", validation.Profile.Username);

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

    [Fact]
    public async Task ProbeLoginKeyVariants_stops_at_first_accepted_formula()
    {
        var handler = new MashhadSsoFakeHandler();
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var results = await service.ProbeLoginKeyVariantsAsync("FinancialAssistant", "53db42619cf3C333b13a18D34fbd9111", "full-secret-from-portal");

        Assert.Single(results);
        Assert.True(results[0].Ok);
        Assert.StartsWith("sha256(secret+time) ASCII hex lower", results[0].Variant);
    }

    [Fact]
    public async Task ProbeLoginKeyVariants_finds_formula_when_sso_only_accepts_upper_hex()
    {
        var expectedUpper = SsoApiSecretHash.ComputeApiSecret("other-secret", "1700000000", "upper");
        var handler = new MashhadSsoFakeHandler(loginKeyResponder: req =>
        {
            var sent = req.Headers.TryGetValues("apiSecret", out var v) ? v.FirstOrDefault() : null;
            var apiName = req.Headers.TryGetValues("apiName", out var n) ? n.FirstOrDefault() : null;
            return apiName == "zavabetapp" && sent == expectedUpper
                ? """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk-upper"}}"""
                : """{"ErrorCode":403,"ErrorMessage":"Client info missmatched","Data":null}""";
        });
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var results = await service.ProbeLoginKeyVariantsAsync("zavabetapp", "other-client", "other-secret");

        var winner = results.FirstOrDefault(r => r.Ok);
        Assert.NotNull(winner);
        Assert.Equal("sha256(secret+time) ASCII hex upper", winner!.Variant);
        Assert.True(results.TakeWhile(r => !r.Ok).All(r => r.ErrorCode == 403));
    }

    [Fact]
    public async Task ProbeLoginKeyVariants_returns_single_getCurrentTime_failure_when_sso_unreachable()
    {
        var handler = new ThrowingHandler();
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var results = await service.ProbeLoginKeyVariantsAsync("FinancialAssistant", "id", "secret");

        Assert.Single(results);
        Assert.Equal("getCurrentTime", results[0].Variant);
        Assert.False(results[0].Ok);
        Assert.Equal(-1, results[0].ErrorCode);
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("No such host is known");
    }

    [Fact]
    public async Task ProbeLoginKeyVariants_reports_all_403_when_credentials_are_wrong()
    {
        var handler = new MashhadSsoFakeHandler(loginKeyJson: """{"ErrorCode":403,"ErrorMessage":"Client info missmatched","Data":null}""");
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var results = await service.ProbeLoginKeyVariantsAsync("FinancialAssistant", "bad", "bad");

        // ۹ فرمول هش + ۳ شکل درخواست (apiName=ClientId، بدون UserType/DomainId، کلید ClientID)
        Assert.Equal(SsoApiSecretHash.ProbeVariants.Count + 3, results.Count);
        Assert.All(results, r =>
        {
            Assert.False(r.Ok);
            Assert.Equal(403, r.ErrorCode);
        });
        Assert.Contains(results, r => r.Variant.StartsWith("apiName = ClientId", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProbeLoginKeyVariants_phase2_finds_apiName_equals_clientId_and_omits_userType_when_asked()
    {
        var expected = SsoApiSecretHash.ComputeApiSecret("s3cret", "1700000000", "lower");
        string? acceptedBody = null;
        var handler = new MashhadSsoFakeHandler(loginKeyResponder: req =>
        {
            var apiName = req.Headers.TryGetValues("apiName", out var n) ? n.FirstOrDefault() : null;
            var sent = req.Headers.TryGetValues("apiSecret", out var v) ? v.FirstOrDefault() : null;
            if (apiName == "client-123" && sent == expected)
            {
                acceptedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk-shape"}}""";
            }
            return """{"ErrorCode":403,"ErrorMessage":"Client info missmatched","Data":null}""";
        });
        var service = CreateServiceWithSso(handler, useLoginKey: true);

        var results = await service.ProbeLoginKeyVariantsAsync("FinancialAssistant", "client-123", "s3cret");

        Assert.Equal(SsoApiSecretHash.ProbeVariants.Count + 1, results.Count);
        var winner = results.Last();
        Assert.True(winner.Ok);
        Assert.StartsWith("apiName = ClientId", winner.Variant);
        Assert.NotNull(acceptedBody);
        Assert.Contains("\"ClientId\":\"client-123\"", acceptedBody);
        Assert.Contains("\"UserType\":0", acceptedBody);
        Assert.Contains("\"DomainID\":0", acceptedBody);
    }

    [Fact]
    public async Task SendLoginKeyProbe_serializes_DomainID_per_sso_doc()
    {
        string? body = null;
        var handler = new MashhadSsoFakeHandler(b => body = b);
        var opts = Options.Create(new SsoAuthOptions { ApiBaseUrl = "https://login.mashhad.ir", LoginState = "test", LoginDomainId = 0 });
        var client = new MashhadSsoApiClient(opts, new NamedHttpClientFactory(handler), NullLogger<MashhadSsoApiClient>.Instance);

        await client.SendLoginKeyProbeAsync("api", "cid", "sec", "1700000000", (s, t) => "hash", default);

        Assert.NotNull(body);
        Assert.Contains("\"DomainID\":0", body);
        Assert.DoesNotContain("DomainId", body);
    }

    [Fact]
    public async Task SendLoginKeyProbe_can_omit_userType_domainId_and_use_ClientID_key()
    {
        string? body = null;
        var handler = new MashhadSsoFakeHandler(b => body = b);
        var opts = Options.Create(new SsoAuthOptions { ApiBaseUrl = "https://login.mashhad.ir", LoginState = "test" });
        var client = new MashhadSsoApiClient(opts, new NamedHttpClientFactory(handler), NullLogger<MashhadSsoApiClient>.Instance);

        await client.SendLoginKeyProbeAsync("api", "cid", "sec", "1700000000", (s, t) => "hash", default, omitUserTypeAndDomain: true, clientIdKey: "ClientID");

        Assert.NotNull(body);
        Assert.Contains("\"ClientID\":\"cid\"", body);
        Assert.Contains("\"Hash\":\"hash\"", body);
        Assert.DoesNotContain("UserType", body);
        Assert.DoesNotContain("DomainId", body);
    }

    private static SsoAuthService CreateServiceWithSso(HttpMessageHandler handler, bool useLoginKey)
    {
        var httpFactory = new NamedHttpClientFactory(handler);
        var opts = Options.Create(new SsoAuthOptions
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
        return new SsoAuthService(
            opts,
            repo,
            mashhad,
            httpFactory,
            NullLogger<SsoAuthService>.Instance,
            SsoAuthServiceTests.StubHost());
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
        private readonly Func<HttpRequestMessage, string>? _loginKeyResponder;

        public MashhadSsoFakeHandler(
            Action<string?>? onBody = null,
            string? loginKeyJson = null,
            Func<HttpRequestMessage, string>? loginKeyResponder = null)
        {
            _onBody = onBody;
            _loginKeyJson = loginKeyJson ?? """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk-123"}}""";
            _loginKeyResponder = loginKeyResponder;
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
                json = _loginKeyResponder?.Invoke(request) ?? _loginKeyJson;
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

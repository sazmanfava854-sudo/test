using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class ShimasAuthService
{
    public const string SsoStateCookieName = "RayvarzResend.SsoState";
    public const string PostLoginReturnCookieName = "RayvarzResend.PostLoginReturn";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ShimasAuthOptions _options;
    private readonly AppUserRepository _users;
    private readonly MashhadSsoApiClient _mashhadSso;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ShimasAuthService> _logger;

    public ShimasAuthService(
        IOptions<ShimasAuthOptions> options,
        AppUserRepository users,
        MashhadSsoApiClient mashhadSso,
        IHttpClientFactory httpClientFactory,
        ILogger<ShimasAuthService> logger)
    {
        _options = options.Value;
        _users = users;
        _mashhadSso = mashhadSso;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public ShimasAuthOptions Options => _options;

    public ShimasAuthStatusDto GetStatus(HttpRequest? request = null)
    {
        var host = request?.Host.Host;
        var preferSso = _options.PreferSsoLoginForHost(host);
        return new ShimasAuthStatusDto
        {
            Enabled = _options.Enabled,
            SsoReady = _options.SsoReady,
            PreferSsoLogin = preferSso,
            LocalLoginAvailable = _options.LocalLoginAvailableForHost(host),
            AllowAdminLocalLoginOnPublicHost = _options.AllowAdminLocalLoginOnPublicHost,
            LoginPath = preferSso ? "/auth/login" : "/login.html",
            CallbackPath = NormalizeCallbackPath(_options.CallbackPath),
            RegisteredCallbackUrl = request != null ? BuildCallbackAbsoluteUrl(request) : ResolvePublicCallbackUrl()
        };
    }

    public string? ResolvePublicCallbackUrl()
    {
        var baseUrl = NormalizePublicBaseUrl(_options.PublicBaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        return $"{baseUrl}{CallbackPathSuffix(NormalizeCallbackPath(_options.CallbackPath))}";
    }

    public string ResolveLoginRedirectPath(HttpRequest? request = null)
    {
        if (_options.PreferSsoLoginForHost(request?.Host.Host))
            return "/auth/login";
        return "/login.html";
    }

    public string BuildExternalLoginUrl(string callbackAbsoluteUrl)
    {
        var clientId = _options.EffectiveClientId;
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("ClientId / lkey هنوز تنظیم نشده است");

        var query = new Dictionary<string, string?>
        {
            [_options.LKeyParameter] = clientId,
            [_options.ClientIdParameter] = clientId,
            [_options.ReturnUrlParameter] = callbackAbsoluteUrl
        };

        return QueryHelpers.AddQueryString(_options.LoginUrl, query);
    }

    public async Task<string> BuildExternalLoginUrlAsync(
        string callbackAbsoluteUrl,
        HttpContext? http = null,
        CancellationToken ct = default)
    {
        if (!_options.UseLoginKeyOnRedirect || !_options.UseMashhadAuthenticationApi)
            return BuildExternalLoginUrl(callbackAbsoluteUrl);

        if (string.IsNullOrWhiteSpace(_options.SigningApiName))
            throw new InvalidOperationException(
                "Auth:Shimas:ApiName = نام کاربری ثبت‌شده در SSO (جدول ۱ ردیف ۲، همان SSOUserName در RuleEngine) — نه ClientId.");

        var state = ResolveLoginState(http);
        RememberLoginState(http, state);

        var keyResult = await _mashhadSso.GetLoginKeyAsync(callbackAbsoluteUrl, state, ct);
        var loginKey = keyResult.Data?.EffectiveLoginKey;
        if (!keyResult.IsSuccess || string.IsNullOrWhiteSpace(loginKey))
        {
            var detail = keyResult.ErrorMessage ?? "loginKey خالی است";
            var hint = keyResult.ErrorCode == 403
                ? " (apiName باید نام کاربری SSO باشد، ClientId و SecretKey جدا هستند)"
                : "";
            _logger.LogWarning(
                "Mashhad loginKey failed (apiName={ApiName}, clientId={ClientId}): {Detail} (code {Code})",
                _options.SigningApiName,
                _options.EffectiveClientId,
                detail,
                keyResult.ErrorCode);

            if (_options.AllowLegacyLoginUrlWithoutLoginKey)
            {
                _logger.LogWarning("Falling back to legacy Login.aspx (lkey + returnUrl) without loginKey");
                return BuildExternalLoginUrl(callbackAbsoluteUrl);
            }

            throw new InvalidOperationException($"دریافت loginKey از SSO ناموفق بود: {detail}{hint}");
        }

        return BuildLoginStartUrl(loginKey);
    }

    public string BuildLoginStartUrl(string loginKey)
    {
        var key = (loginKey ?? "").Trim();
        var template = string.IsNullOrWhiteSpace(_options.LoginStartUrlTemplate)
            ? "https://login.mashhad.ir/Authentication/Start/{loginKey}"
            : _options.LoginStartUrlTemplate.Trim();

        return template.Replace("{loginKey}", key, StringComparison.OrdinalIgnoreCase);
    }

    public bool ValidateReturnedState(HttpContext? http, string? returnedState)
    {
        if (http == null)
            return true;

        var returned = (returnedState ?? "").Trim();
        var cookieState = (http.Request.Cookies[SsoStateCookieName] ?? "").Trim();

        // loginKey flow: state cookie set on /auth/login
        if (!string.IsNullOrWhiteSpace(cookieState))
            return string.Equals(cookieState, returned, StringComparison.Ordinal);

        // Legacy Login.aspx often omits state — do not require LoginState default ("test")
        if (string.IsNullOrWhiteSpace(returned))
            return true;

        var configured = (_options.LoginState ?? "").Trim();
        if (string.IsNullOrWhiteSpace(configured))
            return true;

        return string.Equals(configured, returned, StringComparison.Ordinal);
    }

    private string ResolveLoginState(HttpContext? http)
    {
        if (!string.IsNullOrWhiteSpace(_options.LoginState))
            return _options.LoginState.Trim();

        return Guid.NewGuid().ToString("N");
    }

    private static void RememberLoginState(HttpContext? http, string state)
    {
        if (http == null || string.IsNullOrWhiteSpace(state))
            return;

        http.Response.Cookies.Append(SsoStateCookieName, state, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = http.Request.IsHttps,
            MaxAge = TimeSpan.FromMinutes(20),
            Path = "/"
        });
    }

    public void RememberPostLoginReturn(HttpContext http, string? returnPath = null)
    {
        var path = (returnPath ?? "/").Trim();
        if (path.Length == 0 || !path.StartsWith('/') || path.StartsWith("//", StringComparison.Ordinal))
            path = "/";

        http.Response.Cookies.Append(PostLoginReturnCookieName, path, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = http.Request.IsHttps,
            MaxAge = TimeSpan.FromMinutes(30),
            Path = "/"
        });
    }

    public string ResolvePostLoginRedirect(HttpContext http)
    {
        var raw = (http.Request.Cookies[PostLoginReturnCookieName] ?? "").Trim();
        if (raw.Length == 0 || !raw.StartsWith('/') || raw.StartsWith("//", StringComparison.Ordinal))
            return "/";

        return raw;
    }

    public void ClearSsoFlowCookies(HttpContext http)
    {
        http.Response.Cookies.Delete(SsoStateCookieName, new CookieOptions { Path = "/" });
        http.Response.Cookies.Delete(PostLoginReturnCookieName, new CookieOptions { Path = "/" });
    }

    public string BuildCallbackAbsoluteUrl(HttpRequest request)
    {
        var configured = NormalizePublicBaseUrl(_options.PublicBaseUrl);
        var suffix = CallbackPathSuffix(NormalizeCallbackPath(_options.CallbackPath));
        if (!string.IsNullOrWhiteSpace(configured))
            return $"{configured}{suffix}";

        return $"{request.Scheme}://{request.Host}{suffix}";
    }

    /// <summary>بازگشت SSO روی ریشه سایت (مثلاً https://city.mashhad.ir:5065) با querystring توکن.</summary>
    public bool IsSsoCallbackHttpRequest(HttpRequest request)
    {
        if (!HttpMethods.IsGet(request.Method))
            return false;

        if (!MatchesCallbackRequestPath(request.Path.Value ?? ""))
            return false;

        return QueryLooksLikeSsoCallback(request.Query);
    }

    public bool QueryLooksLikeSsoCallback(IQueryCollection query)
    {
        var payload = ParseCallbackQuery(query);
        if (payload.RefreshToken.Length < _options.MinRefreshTokenLength)
            return false;

        return !string.IsNullOrWhiteSpace(payload.Username)
            || !string.IsNullOrWhiteSpace(payload.Domain);
    }

    private bool MatchesCallbackRequestPath(string path)
    {
        var callbackPath = NormalizeCallbackPath(_options.CallbackPath);
        if (path.Equals(callbackPath, StringComparison.OrdinalIgnoreCase))
            return true;

        if (callbackPath == "/" && path.Equals("/index.html", StringComparison.OrdinalIgnoreCase))
            return true;

        return path.Equals("/auth/callback", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>پارامترهای بازگشت از login.mashhad.ir / سامزان: username + refresh_token.</summary>
    public ShimasCallbackPayload ParseCallbackQuery(IQueryCollection query)
    {
        var username = ReadQuery(query,
            "username", "userName", "UserName",
            "nationalId", "NationalId", "nationalCode", "NationalCode", "code");
        var domain = ReadQuery(query,
            "domain", "Domain", "sAMAccountName", "samAccountName", "accountName", "AccountName");
        var refreshToken = ReadQuery(query,
            "refresh_token", "refreshToken", "RefreshToken",
            "token", "Token", "access_token", "accessToken");
        var state = ReadQuery(query, "state", "State");

        var normalizedDomain = AppUserDomainNormalizer.Normalize(domain);
        var normalizedUsername = AppUserDomainNormalizer.Normalize(username);
        if (string.IsNullOrEmpty(normalizedUsername))
            normalizedUsername = normalizedDomain;

        return new ShimasCallbackPayload
        {
            Username = normalizedUsername,
            Domain = normalizedDomain,
            RefreshToken = refreshToken,
            State = state
        };
    }

    public async Task<ShimasValidationResult> ValidateAsync(
        string username,
        string refreshToken,
        CancellationToken ct = default) =>
        await ValidateAsync(username, refreshToken, alternateIdentity: null, ct);

    public async Task<ShimasValidationResult> ValidateAsync(
        string username,
        string refreshToken,
        string? alternateIdentity,
        CancellationToken ct = default)
    {
        var normalizedUsername = (username ?? "").Trim();
        if (string.IsNullOrWhiteSpace(normalizedUsername))
            normalizedUsername = AppUserDomainNormalizer.Normalize(alternateIdentity ?? "");
        var normalizedToken = (refreshToken ?? "").Trim();

        if (string.IsNullOrWhiteSpace(normalizedUsername))
            return Fail("username خالی است");

        if (normalizedToken.Length < _options.MinRefreshTokenLength)
            return Fail("refresh_token نامعتبر است");

        if (_options.UseMashhadAuthenticationApi)
            return await ValidateViaMashhadApiAsync(normalizedUsername, normalizedToken, alternateIdentity, ct);

        if (!string.IsNullOrWhiteSpace(_options.ValidateTokenUrl))
            return await ValidateRemoteAsync(normalizedUsername, normalizedToken, ct);

        _logger.LogWarning(
            "Shimas ValidateTokenUrl تنظیم نشده — فقط بررسی اولیه refresh_token انجام شد برای {Username}",
            MaskUsername(normalizedUsername));

        return new ShimasValidationResult
        {
            Success = true,
            UsedRemoteApi = false,
            Profile = BuildProfileFromUsername(normalizedUsername)
        };
    }

    private async Task<ShimasValidationResult> ValidateViaMashhadApiAsync(
        string username,
        string refreshToken,
        string? alternateIdentity,
        CancellationToken ct)
    {
        try
        {
            var tokenUsernames = BuildAccessTokenUsernameCandidates(username, alternateIdentity);
            MashhadSsoResult<MashhadAccessTokenData>? tokenResult = null;
            string? tokenUsernameUsed = null;
            foreach (var candidate in tokenUsernames)
            {
                tokenResult = await _mashhadSso.GetAccessTokenAsync(refreshToken, candidate, ct);
                var accessToken = tokenResult.Data?.AccessToken?.Trim();
                if (tokenResult.IsSuccess && !string.IsNullOrWhiteSpace(accessToken))
                {
                    tokenUsernameUsed = candidate;
                    break;
                }
            }

            var accessTokenFinal = tokenResult?.Data?.AccessToken?.Trim();
            if (tokenResult == null || !tokenResult.IsSuccess || string.IsNullOrWhiteSpace(accessTokenFinal))
            {
                var detail = tokenResult?.ErrorMessage ?? "AccessToken خالی است";
                _logger.LogWarning(
                    "Mashhad getAccessToken failed for {Username} (tried {Count} identities): {Detail} (code {Code})",
                    MaskUsername(username),
                    tokenUsernames.Count,
                    detail,
                    tokenResult?.ErrorCode ?? -1);
                return Fail($"اعتبارسنجی SSO ناموفق بود: {detail}");
            }

            var infoResult = await _mashhadSso.GetUserInfoAsync(accessTokenFinal, ct);
            if (!infoResult.IsSuccess || infoResult.Data == null)
            {
                var detail = infoResult.ErrorMessage ?? "پاسخ getUserInfo خالی است";
                _logger.LogWarning("Mashhad getUserInfo failed: {Detail}", detail);
                return Fail($"دریافت اطلاعات کاربر از SSO ناموفق بود: {detail}");
            }

            var profile = MapMashhadUserInfo(infoResult.Data, tokenUsernameUsed ?? username);
            return new ShimasValidationResult
            {
                Success = true,
                UsedRemoteApi = true,
                Profile = profile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mashhad SSO validation error for {Username}", MaskUsername(username));
            return Fail("خطا در ارتباط با API احراز هویت سازمان");
        }
    }

    private static List<string> BuildAccessTokenUsernameCandidates(string username, string? alternateIdentity)
    {
        var list = new List<string>();
        void Add(string? value)
        {
            var text = (value ?? "").Trim();
            if (text.Length == 0)
                return;
            if (list.Any(s => s.Equals(text, StringComparison.OrdinalIgnoreCase)))
                return;
            list.Add(text);
        }

        Add(username);
        Add(alternateIdentity);
        Add(AppUserDomainNormalizer.Normalize(username));
        Add(AppUserDomainNormalizer.Normalize(alternateIdentity));
        return list;
    }

    private static ShimasUserProfile MapMashhadUserInfo(MashhadUserInfoData data, string fallbackUsername)
    {
        var basic = data.OldSSO_UserInfo?.basicInfo;
        var domainAccount = AppUserDomainNormalizer.Normalize(
            basic?.username ?? data.UserName ?? fallbackUsername);
        var nationalId = (data.NationalCode ?? basic?.nationalCode ?? "").Trim();
        var loginUsername = AppUserInputNormalizer.IsValidNationalId(nationalId)
            ? nationalId
            : AppUserDomainNormalizer.Normalize(data.UserName ?? fallbackUsername);

        if (string.IsNullOrEmpty(loginUsername))
            loginUsername = domainAccount;

        return new ShimasUserProfile
        {
            Username = loginUsername,
            Domain = domainAccount,
            NationalId = nationalId,
            FirstName = (data.FName ?? basic?.firstname ?? "").Trim(),
            LastName = (data.LName ?? basic?.surname ?? "").Trim()
        };
    }

    public async Task<AppUserRecord?> ResolveOrCreateUserAsync(
        ShimasUserProfile profile,
        CancellationToken ct = default)
    {
        var username = (profile.Username ?? "").Trim();
        if (username.Length == 0)
            return null;

        var existing = await _users.FindBySsoIdentityAsync(username, ct)
            ?? await _users.FindBySsoIdentityAsync(profile.Domain, ct)
            ?? await _users.FindByUsernameAsync(username, ct);
        if (existing != null)
            return existing.IsActive ? existing : null;

        if (!_options.AutoProvisionUsers)
        {
            _logger.LogWarning("SSO user {Username} not found and AutoProvisionUsers=false", MaskUsername(username));
            return null;
        }

        return await _users.CreateSsoUserAsync(profile, ct);
    }

    private async Task<ShimasValidationResult> ValidateRemoteAsync(
        string username,
        string refreshToken,
        CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(nameof(ShimasAuthService));
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.ValidateTokenUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = JsonContent.Create(new
            {
                username,
                refresh_token = refreshToken,
                client_id = _options.EffectiveClientId,
                client_secret = _options.ClientSecret,
                lkey = _options.EffectiveClientId
            });

            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Shimas token validation failed ({Status}) for {Username}",
                    (int)response.StatusCode,
                    MaskUsername(username));
                return Fail("اعتبارسنجی refresh_token ناموفق بود");
            }

            var profile = await TryLoadProfileAsync(username, refreshToken, ct)
                ?? ParseProfileFromJson(body, username)
                ?? BuildProfileFromUsername(username);

            profile.Username = username;
            return new ShimasValidationResult
            {
                Success = true,
                UsedRemoteApi = true,
                Profile = profile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shimas remote validation error for {Username}", MaskUsername(username));
            return Fail("خطا در ارتباط با سرویس احراز هویت سازمان");
        }
    }

    private async Task<ShimasUserProfile?> TryLoadProfileAsync(
        string username,
        string refreshToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.UserProfileUrl))
            return null;

        var client = _httpClientFactory.CreateClient(nameof(ShimasAuthService));
        using var request = new HttpRequestMessage(HttpMethod.Get, _options.UserProfileUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {refreshToken}");
        request.Headers.TryAddWithoutValidation("X-Username", username);

        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        var body = await response.Content.ReadAsStringAsync(ct);
        return ParseProfileFromJson(body, username);
    }

    private static ShimasUserProfile? ParseProfileFromJson(string json, string username)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new ShimasUserProfile
            {
                Username = ReadJsonString(root, "username", "userName", "UserName") ?? username,
                FirstName = ReadJsonString(root, "firstName", "FirstName", "name", "Name") ?? "",
                LastName = ReadJsonString(root, "lastName", "LastName", "family", "Family") ?? "",
                NationalId = ReadJsonString(root, "nationalId", "NationalId", "nationalCode", "NationalCode") ?? "",
                Position = ReadJsonString(root, "position", "Position", "title", "Title") ?? "",
                District = ReadJsonString(root, "district", "District", "branch", "Branch") ?? ""
            };
        }
        catch
        {
            return null;
        }
    }

    private static ShimasUserProfile BuildProfileFromUsername(string username)
    {
        var parts = username.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new ShimasUserProfile
        {
            Username = username,
            Domain = AppUserDomainNormalizer.Normalize(username),
            FirstName = parts.Length > 0 ? parts[0] : username,
            LastName = parts.Length > 1 ? parts[1] : "کاربر"
        };
    }

    private static ShimasValidationResult Fail(string error) => new()
    {
        Success = false,
        Error = error
    };

    private static string NormalizeCallbackPath(string? path)
    {
        var value = (path ?? "/auth/callback").Trim();
        if (value.Length == 0 || value == "/")
            return "/";

        return value.StartsWith('/') ? value : "/" + value;
    }

    /// <summary>برای CallbackPath=/ فقط PublicBaseUrl — بدون /auth/callback.</summary>
    private static string CallbackPathSuffix(string normalizedPath) =>
        normalizedPath == "/" ? "" : normalizedPath;

    private static string NormalizePublicBaseUrl(string? value)
    {
        var trimmed = (value ?? "").Trim();
        if (trimmed.Length == 0)
            return "";

        return trimmed.EndsWith('/') ? trimmed[..^1] : trimmed;
    }

    private static string ReadQuery(IQueryCollection query, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!query.TryGetValue(key, out var values))
                continue;

            var text = values.ToString().Trim();
            if (!string.IsNullOrEmpty(text))
                return text;
        }

        return "";
    }

    private static string? ReadJsonString(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var prop))
            {
                var text = prop.GetString()?.Trim();
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
        }

        return null;
    }

    private static string MaskUsername(string username)
    {
        if (username.Length <= 4) return "***";
        return username[..2] + "***" + username[^2..];
    }
}

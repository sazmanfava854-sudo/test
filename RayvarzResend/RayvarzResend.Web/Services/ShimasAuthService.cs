using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
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
    private readonly bool _isDevelopment;

    public ShimasAuthService(
        IOptions<ShimasAuthOptions> options,
        AppUserRepository users,
        MashhadSsoApiClient mashhadSso,
        IHttpClientFactory httpClientFactory,
        ILogger<ShimasAuthService> logger,
        IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _users = users;
        _mashhadSso = mashhadSso;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _isDevelopment = hostEnvironment.IsDevelopment();
    }

    public ShimasAuthOptions Options => _options;

    public ShimasAuthStatusDto GetStatus(HttpRequest? request = null)
    {
        var host = request?.Host.Host;
        var allowLoopbackSso = _options.AllowSsoOnLoopbackForDebug && ShimasAuthOptions.IsLoopbackHost(host);
        var preferSso = _options.PreferSsoLoginForHost(host);
        var loginPath = ResolveLoginPath(request);
        var publicSso = UsesPublicSsoLoginUrl(request);
        return new ShimasAuthStatusDto
        {
            Enabled = _options.Enabled,
            SsoReady = _options.SsoReady,
            PreferSsoLogin = preferSso,
            AllowSsoOnLoopbackForDebug = allowLoopbackSso,
            PublicSsoLoginUrl = publicSso ? loginPath : null,
            LocalLoginAvailable = _options.LocalLoginAvailableForHost(host),
            AllowAdminLocalLoginOnPublicHost = _options.AllowAdminLocalLoginOnPublicHost,
            LoginPath = loginPath,
            PostLoginDefaultPath = NormalizePostLoginPath(_options.PostLoginDefaultPath),
            CallbackPath = NormalizeCallbackPath(_options.CallbackPath),
            RegisteredCallbackUrl = request != null ? BuildCallbackAbsoluteUrl(request) : ResolvePublicCallbackUrl(),
            SsoReturnUrlForPortal = ResolveSsoReturnUrlForPortal(request),
            SigningApiName = string.IsNullOrWhiteSpace(_options.SigningApiName) ? null : _options.SigningApiName,
            ClientIdHint = SsoCredentialMask.MaskId(_options.EffectiveClientId),
            ClientSecretLooksShort = SsoCredentialMask.SecretLooksTooShort(_options.ClientSecret),
            SsoOutboundMap = SsoOutboundFieldMap.Describe(_options)
        };
    }

    public string? ResolvePublicCallbackUrl()
    {
        var baseUrl = NormalizePublicBaseUrl(_options.PublicBaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        return BuildCallbackAbsoluteUrlFromParts(baseUrl, "", NormalizeCallbackPath(_options.CallbackPath));
    }

    /// <summary>آدرسی که به برنامه‌نویس SSO برای «برگشت آدرس» می‌دهید.</summary>
    public string ResolveSsoReturnUrlForPortal(HttpRequest? request = null)
    {
        if (request != null)
            return BuildCallbackAbsoluteUrl(request);

        var registered = NormalizePublicBaseUrl(_options.SsoRegisteredReturnUrl);
        if (!string.IsNullOrWhiteSpace(registered))
            return registered;

        return ResolvePublicCallbackUrl() ?? "";
    }

    public string DescribeLoginStartForPortal(HttpRequest? request = null)
    {
        var callback = ResolveSsoReturnUrlForPortal(request);
        return BuildExternalLoginUrl(callback);
    }

    public string ResolveLoginRedirectPath(HttpRequest? request = null)
    {
        return ResolveLoginPath(request);
    }

    /// <summary>روی localhost با PublicBaseUrl — لینک SSO برای کاربر نهایی (city). در Development خاموش است تا F5 به مشهد redirect نشود.</summary>
    public bool UsesPublicSsoLoginUrl(HttpRequest? request) =>
        request != null
        && !_isDevelopment
        && ShimasAuthOptions.IsLoopbackHost(request.Host.Host)
        && !string.IsNullOrWhiteSpace(NormalizePublicBaseUrl(_options.PublicBaseUrl))
        && _options.SsoReady;

    public string ResolveLoginPath(HttpRequest? request)
    {
        if (UsesPublicSsoLoginUrl(request))
            return CombinePublicBaseUrl(request!, "/auth/login");

        if (request != null
            && _isDevelopment
            && ShimasAuthOptions.IsLoopbackHost(request.Host.Host)
            && _options.SsoReady)
            return "/auth/login";

        return _options.PreferSsoLoginForHost(request?.Host.Host) ? "/auth/login" : "/login.html";
    }

    public string CombinePublicBaseUrl(HttpRequest request, string relativePath)
    {
        var configured = NormalizePublicBaseUrl(_options.PublicBaseUrl);
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException("PublicBaseUrl تنظیم نشده است");

        var appPath = ResolveApplicationPath(request);
        var rel = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        return $"{configured}{appPath}{rel}";
    }

    public string BuildExternalLoginUrl(string callbackAbsoluteUrl)
    {
        var clientId = _options.EffectiveClientId;
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("ClientId / lkey هنوز تنظیم نشده است");

        // Login.aspx (ASP.NET) کلیدهای querystring را بدون حساسیت به حروف می‌خواند؛ returnUrl+ReturnUrl+returnurl
        // به «url,url,url» تبدیل می‌شود، با برگشت آدرس ثبت‌شده تطبیق نمی‌کند و SSO روی Profile.aspx می‌ماند.
        var returnUrlKey = string.IsNullOrWhiteSpace(_options.ReturnUrlParameter) ? "ReturnUrl" : _options.ReturnUrlParameter.Trim();
        var query = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            [_options.LKeyParameter] = clientId,
            [_options.ClientIdParameter] = clientId,
            [returnUrlKey] = callbackAbsoluteUrl
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

        var keyResult = await _mashhadSso.GetLoginKeyAsync(callbackAbsoluteUrl, state, ct);
        var loginKey = keyResult.Data?.EffectiveLoginKey;
        if (!keyResult.IsSuccess || string.IsNullOrWhiteSpace(loginKey))
        {
            var detail = keyResult.ErrorMessage ?? "loginKey خالی است";
            var hint = keyResult.ErrorCode == 403
                ? " (apiName باید نام کاربری SSO باشد، ClientId و SecretKey جدا هستند)"
                : "";
            _logger.LogWarning(
                "Mashhad loginKey failed (apiName={ApiName}, clientId={ClientId}, returnUrl={ReturnUrl}): {Detail} (code {Code})",
                _options.SigningApiName,
                _options.EffectiveClientId,
                callbackAbsoluteUrl,
                detail,
                keyResult.ErrorCode);

            if (_options.AllowLegacyLoginUrlWithoutLoginKey)
            {
                if (http != null)
                    ClearSsoFlowCookies(http);
                _logger.LogWarning("Falling back to legacy Login.aspx (lkey + returnUrl) without loginKey");
                return BuildExternalLoginUrl(callbackAbsoluteUrl);
            }

            throw new InvalidOperationException(BuildLoginKeyFailureMessage(detail, hint));
        }

        RememberLoginState(http, state);
        _logger.LogInformation(
            "SSO loginKey OK — redirect to Start/{{loginKey}}, returnUrl={ReturnUrl}, state length={StateLen}",
            callbackAbsoluteUrl,
            state.Length);
        return BuildLoginStartUrl(loginKey);
    }

    /// <summary>تشخیص: آیا SSO برای این ClientId/Secret/apiName یک loginKey می‌دهد؟ (بدون هدایت کاربر)</summary>
    public async Task<SsoLoginKeyDiagnostics> DiagnoseLoginKeyAsync(string callbackAbsoluteUrl, CancellationToken ct = default)
    {
        var diag = new SsoLoginKeyDiagnostics
        {
            ApiBaseUrl = _options.ApiBaseUrl,
            ApiName = _options.SigningApiName,
            ClientIdMasked = SsoCredentialMask.MaskId(_options.EffectiveClientId),
            ClientIdLength = _options.EffectiveClientId.Length,
            ClientSecretLength = (_options.ClientSecret ?? "").Trim().Length,
            UseLoginKeyOnRedirect = _options.UseLoginKeyOnRedirect,
            ReturnUrlRegisteredInPortal = callbackAbsoluteUrl
        };

        if (!_options.UseMashhadAuthenticationApi)
        {
            diag.Error = "ApiBaseUrl / ApiName / ClientSecret کامل نیست — روش loginKey فعال نمی‌شود و فقط Login.aspx استفاده می‌شود.";
            return diag;
        }

        if (SsoCredentialMask.SecretLooksTooShort(_options.ClientSecret))
        {
            diag.Error =
                $"ClientSecret در appsettings فقط {diag.ClientSecretLength} کاراکتر است — احتمالاً placeholder (مثل D2fbf) است، نه SecretKey کامل پورتال SSO. "
                + "SecretKey را از ادمین SSO بگیرید و در Auth:Shimas:ClientSecret (یا appsettings.Development.json روی PC) بگذارید؛ بعد recycle IIS.";
            return diag;
        }

        try
        {
            var time = await _mashhadSso.GetCurrentTimeAsync(ct);
            diag.GetCurrentTimeOk = time.IsSuccess && !string.IsNullOrWhiteSpace(time.Data);
            if (!diag.GetCurrentTimeOk)
            {
                diag.Error = $"getCurrentTime ناموفق: {time.ErrorMessage} (code {time.ErrorCode})";
                return diag;
            }

            var key = await _mashhadSso.GetLoginKeyAsync(callbackAbsoluteUrl, ResolveLoginState(null), ct);
            diag.LoginKeyErrorCode = key.ErrorCode;
            diag.LoginKeyErrorMessage = key.ErrorMessage;
            var loginKey = key.Data?.EffectiveLoginKey;
            diag.LoginKeyOk = key.IsSuccess && !string.IsNullOrWhiteSpace(loginKey);
            if (diag.LoginKeyOk)
            {
                diag.SuggestedLoginUrl = BuildLoginStartUrl(loginKey);
                diag.StartUrlSample = BuildLoginStartUrl(SsoCredentialMask.MaskId(loginKey));
            }
            else
            {
                if (_options.AllowLegacyLoginUrlWithoutLoginKey)
                    diag.SuggestedLoginUrl = BuildExternalLoginUrl(callbackAbsoluteUrl);

                diag.Error = key.ErrorCode == 403
                    ? "SSO می‌گوید Client info mismatch — ClientId/ClientSecret/apiName با ثبت پورتال یکی نیست (Secret کامل؟ apiName = نام کاربری SSO؟)."
                    : $"loginKey ناموفق: {key.ErrorMessage} (code {key.ErrorCode})";
            }
        }
        catch (Exception ex)
        {
            diag.Error = $"خطا در ارتباط با {_options.ApiBaseUrl}: {ex.Message}";
        }

        return diag;
    }

    /// <summary>مقادیر دقیق هدر/بدنه loginKey همان لحظه (برای دیباگ؛ SecretKey برگردانده نمی‌شود).</summary>
    public async Task<SsoSigningPreviewDto> PreviewLoginKeySigningAsync(CancellationToken ct = default)
    {
        var encoding = string.Equals(_options.HashEncoding, "upper", StringComparison.OrdinalIgnoreCase) ? "upper" : "lower";
        var preview = new SsoSigningPreviewDto
        {
            HashEncoding = encoding,
            ClientSecretLength = (_options.ClientSecret ?? "").Trim().Length,
            NoteFa = "apiSecret و Hash یکی هستند. requestTime و Time یکی هستند. ClientSecret در خروجی نیست."
        };

        var time = await _mashhadSso.GetCurrentTimeAsync(ct);
        if (!time.IsSuccess || string.IsNullOrWhiteSpace(time.Data))
        {
            preview.NoteFa = $"getCurrentTime ناموفق: {time.ErrorMessage} (code {time.ErrorCode})";
            return preview;
        }

        var material = MashhadSsoSigning.CreateMaterial(
            _options.SigningApiName,
            _options.ClientSecret,
            time.Data,
            encoding,
            _options.EffectiveApiSecretConcatOrder);

        preview.Headers = new SsoSigningHeaderPreview
        {
            ApiName = material.ApiName,
            RequestTime = material.RequestTime,
            ApiSecret = material.ApiSecret
        };
        preview.Body = new SsoSigningBodyPreview
        {
            Time = material.RequestTime,
            Hash = material.ApiSecret,
            ClientId = _options.EffectiveLoginKeyBodyClientId,
            State = string.IsNullOrWhiteSpace(_options.LoginState) ? "test" : _options.LoginState.Trim(),
            UserType = _options.LoginUserType,
            DomainId = _options.LoginDomainId
        };

        return preview;
    }

    /// <summary>
    /// همهٔ فرمول‌های هش را با اعتبار داده‌شده می‌آزماید. اگر با اعتبار RuleEngine هم همه 403 بدهند،
    /// فرمول ما با SSO فرق دارد؛ اگر با RuleEngine یکی OK شد و با FinancialAssistant نه، ثبت پورتال مشکل دارد.
    /// </summary>
    public async Task<List<SsoLoginKeyProbeResult>> ProbeLoginKeyVariantsAsync(
        string apiName,
        string clientId,
        string secret,
        CancellationToken ct = default,
        bool allVariants = true)
    {
        var results = new List<SsoLoginKeyProbeResult>();

        MashhadSsoResult<string> time;
        try
        {
            time = await _mashhadSso.GetCurrentTimeAsync(ct);
        }
        catch (Exception ex)
        {
            time = new MashhadSsoResult<string> { ErrorCode = -1, ErrorMessage = ex.Message };
        }

        if (!time.IsSuccess || string.IsNullOrWhiteSpace(time.Data))
        {
            results.Add(new SsoLoginKeyProbeResult
            {
                Variant = "getCurrentTime",
                Ok = false,
                ErrorCode = time.ErrorCode,
                ErrorMessage = time.ErrorMessage ?? "getCurrentTime ناموفق بود — SSO از این سرور در دسترس نیست"
            });
            return results;
        }

        var requestTime = time.Data.Trim();
        if (!allVariants)
        {
            // سند ص ۲۰: فقط SHA256(appSecretKey + time) — یک درخواست، بدون آزمودن فرمول‌های دیگر.
            var (docName, docCompute) = SsoApiSecretHash.ProbeVariants[0];
            results.Add(await RunProbeAsync(docName, apiName, clientId, secret, requestTime, docCompute, ct));
            return results;
        }

        foreach (var (name, compute) in SsoApiSecretHash.ProbeVariants)
        {
            var item = await RunProbeAsync(name, apiName, clientId, secret, requestTime, compute, ct);
            results.Add(item);
            if (item.Ok)
                return results;
        }

        // فاز ۲: هش درست ولی شکل درخواست/apiName فرق دارد؟ (فقط با فرمول اصلی)
        var primary = SsoApiSecretHash.ProbeVariants[0].Compute;
        var shapeVariants = new (string Name, string ApiName, bool OmitUserTypeDomain, string ClientIdKey)[]
        {
            ("apiName = ClientId (sha256 lower)", clientId, false, "ClientId"),
            ("بدون UserType/DomainId (sha256 lower)", apiName, true, "ClientId"),
            ("کلید ClientID با حروف بزرگ (sha256 lower)", apiName, false, "ClientID")
        };
        foreach (var v in shapeVariants)
        {
            var item = await RunProbeAsync(v.Name, v.ApiName, clientId, secret, requestTime, primary, ct, v.OmitUserTypeDomain, v.ClientIdKey);
            results.Add(item);
            if (item.Ok)
                break;
        }

        return results;
    }

    private async Task<SsoLoginKeyProbeResult> RunProbeAsync(
        string name,
        string apiName,
        string clientId,
        string secret,
        string requestTime,
        Func<string, string, string> compute,
        CancellationToken ct,
        bool omitUserTypeDomain = false,
        string clientIdKey = "ClientId")
    {
        var item = new SsoLoginKeyProbeResult { Variant = name };
        try
        {
            var r = await _mashhadSso.SendLoginKeyProbeAsync(apiName, clientId, secret, requestTime, compute, ct, omitUserTypeDomain, clientIdKey);
            item.ErrorCode = r.ErrorCode;
            item.ErrorMessage = r.ErrorMessage;
            item.Ok = r.IsSuccess && !string.IsNullOrWhiteSpace(r.Data?.EffectiveLoginKey);
        }
        catch (Exception ex)
        {
            item.ErrorCode = -1;
            item.ErrorMessage = ex.Message;
        }

        return item;
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
        var path = (returnPath ?? _options.PostLoginDefaultPath ?? "/").Trim();
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
            return NormalizePostLoginPath(_options.PostLoginDefaultPath);

        return raw;
    }

    public void ClearSsoFlowCookies(HttpContext http)
    {
        http.Response.Cookies.Delete(SsoStateCookieName, new CookieOptions { Path = "/" });
        http.Response.Cookies.Delete(PostLoginReturnCookieName, new CookieOptions { Path = "/" });
    }

    public string BuildCallbackAbsoluteUrl(HttpRequest request)
    {
        var registered = NormalizePublicBaseUrl(_options.SsoRegisteredReturnUrl);
        if (!string.IsNullOrWhiteSpace(registered))
            return registered;

        var configured = NormalizePublicBaseUrl(_options.PublicBaseUrl);
        var appPath = ResolveApplicationPath(request);
        var callbackPath = NormalizeCallbackPath(_options.CallbackPath);

        if (!string.IsNullOrWhiteSpace(configured))
            return BuildCallbackAbsoluteUrlFromParts(configured, appPath, callbackPath);

        var hostBase = $"{request.Scheme}://{request.Host}";
        return BuildCallbackAbsoluteUrlFromParts(hostBase, appPath, callbackPath);
    }

    private string ResolveApplicationPath(HttpRequest request)
    {
        var configured = NormalizeApplicationPath(_options.ApplicationPath);
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        var pathBase = (request.PathBase.Value ?? "").Trim();
        return NormalizeApplicationPath(pathBase);
    }

    private static string BuildCallbackAbsoluteUrlFromParts(string hostOrBase, string appPath, string callbackPath)
    {
        var baseUrl = NormalizePublicBaseUrl(hostOrBase);
        var suffix = CallbackPathSuffix(callbackPath);
        return $"{baseUrl}{appPath}{suffix}";
    }

    private static string NormalizeApplicationPath(string? path)
    {
        var value = (path ?? "").Trim();
        if (value.Length == 0 || value == "/")
            return "";

        return value.StartsWith('/') ? value.TrimEnd('/') : "/" + value.TrimEnd('/');
    }

    /// <summary>بازگشت SSO روی ریشه سایت (مثلاً https://city.mashhad.ir:5065) با querystring توکن.</summary>
    public bool IsSsoCallbackHttpRequest(HttpRequest request)
    {
        var probe = ProbeSsoCallbackHttpRequest(request);
        return probe.IsSsoCallbackHttpRequest;
    }

    public SsoCallbackProbeDto ProbeSsoCallbackHttpRequest(HttpRequest request)
    {
        var path = request.Path.Value ?? "";
        var pathBase = (request.PathBase.Value ?? "").Trim();
        var effectivePath = ResolveRequestPathForCallback(request);
        var payload = ParseCallbackQuery(request.Query);
        var isGet = HttpMethods.IsGet(request.Method);
        var pathMatches = MatchesCallbackRequestPath(effectivePath)
            || (!string.Equals(effectivePath, path, StringComparison.Ordinal)
                && MatchesCallbackRequestPath(path));
        var tokenOk = payload.RefreshToken.Length >= _options.MinRefreshTokenLength;
        var hasIdentity = !string.IsNullOrWhiteSpace(payload.Username)
            || !string.IsNullOrWhiteSpace(payload.Domain);
        var isCallback = isGet && pathMatches && tokenOk && hasIdentity;

        string? rejection = null;
        if (!isCallback)
        {
            if (!isGet)
                rejection = "متد درخواست GET نیست — callback SSO فقط با GET شناسایی می‌شود.";
            else if (!pathMatches)
                rejection = $"مسیر «{effectivePath}» با CallbackPath «{NormalizeCallbackPath(_options.CallbackPath)}» (یا /auth/callback) جور نیست."
                    + (effectivePath == "/" || effectivePath.Equals("/index.html", StringComparison.OrdinalIgnoreCase)
                        ? " اگر توکن در query هست ولی این پیام را می‌بینید، appsettings را به‌روز کنید (شاخه net7: / هم برای CallbackPath=/management پذیرفته می‌شود)."
                        : "");
            else if (!tokenOk)
                rejection = $"refresh_token در query نیست یا کوتاه‌تر از MinRefreshTokenLength ({_options.MinRefreshTokenLength}) است.";
            else if (!hasIdentity)
                rejection = "username یا domain در querystring نیست — SSO باید ?username=...&refresh_token=... برگرداند.";
        }

        return new SsoCallbackProbeDto
        {
            Method = request.Method,
            Path = path,
            PathBase = pathBase,
            EffectivePath = effectivePath,
            ConfiguredCallbackPath = NormalizeCallbackPath(_options.CallbackPath),
            IsGet = isGet,
            PathMatchesCallback = pathMatches,
            RefreshTokenLength = payload.RefreshToken.Length,
            MinRefreshTokenLength = _options.MinRefreshTokenLength,
            HasUsernameOrDomain = hasIdentity,
            IsSsoCallbackHttpRequest = isCallback,
            RejectionReasonFa = rejection,
            QueryKeys = request.Query.Keys.Take(32).ToArray()
        };
    }

    public string ResolveRequestPathForCallback(HttpRequest request)
    {
        var path = request.Path.Value ?? "";
        var pathBase = (request.PathBase.Value ?? "").TrimEnd('/');
        if (pathBase.Length == 0)
            return path;

        if (path.StartsWith('/'))
            return pathBase + path;

        return pathBase + "/" + path;
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

        // SSO ممکن است با /management یا املای قدیمی /MANAGMENT ثبت شده باشد
        if (ManagementHubPaths.IsHubPage(callbackPath.TrimEnd('/')) && ManagementHubPaths.IsHubPage(path))
            return true;

        // برگشت آدرس ثبت‌شده /management ولی SSO گاهی به ریشه ?username&refresh_token می‌فرستد
        if (ManagementHubPaths.IsHubPage(callbackPath.TrimEnd('/'))
            && (path.Equals("/", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/index.html", StringComparison.OrdinalIgnoreCase)))
            return true;

        if (callbackPath == "/" && path.Equals("/index.html", StringComparison.OrdinalIgnoreCase))
            return true;

        return path.Equals("/auth/callback", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePostLoginPath(string? path)
    {
        var value = (path ?? "/").Trim();
        if (value.Length == 0 || !value.StartsWith('/') || value.StartsWith("//", StringComparison.Ordinal))
            return "/";
        return value;
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
        // همان شناسهٔ ورود SSO و فرم محلی: اول دامین سازمانی (alidoost-pa)، بعد کد ملی
        var loginUsername = AppUserDomainNormalizer.IsValid(domainAccount)
            ? domainAccount
            : AppUserInputNormalizer.IsValidNationalId(nationalId)
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

    private string BuildLoginKeyFailureMessage(string detail, string hint)
    {
        var secretHint = SsoCredentialMask.SecretLooksTooShort(_options.ClientSecret)
            ? " ClientSecret/SSOSecret در appsettings خیلی کوتاه است — مقدار کامل SecretKey پورتال SSO را بگذارید (نه چند حرف اول)."
            : "";

        return
            $"دریافت loginKey از SSO ناموفق بود: {detail}{hint}.{secretHint} " +
            $"ارسال‌شده: apiName='{_options.SigningApiName}', clientId='{SsoCredentialMask.MaskId(_options.EffectiveClientId)}'. " +
            "مثل RuleEngine: Settings.SSOUserName→ApiName، SSOClientId→ClientId، SSOSecret→ClientSecret (ثبت همان سامانه دستیار مالی، نه ضوابط).";
    }
}

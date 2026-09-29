using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.Services;

public sealed class MashhadSsoApiClient
{
    public const string HttpClientName = "MashhadSsoApi";

    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ShimasAuthOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MashhadSsoApiClient> _logger;

    public MashhadSsoApiClient(
        IOptions<ShimasAuthOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<MashhadSsoApiClient> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiBaseUrl)
        && !string.IsNullOrWhiteSpace(_options.SigningApiName)
        && _options.HasClientSecret;

    /// <summary>تنها فراخوانی SSO بدون هدر apiName / requestTime / apiSecret (بند ۳ سند).</summary>
    public async Task<MashhadSsoResult<string>> GetCurrentTimeAsync(CancellationToken ct = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync(Combine("/api/Authentication/getCurrentTime"), ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return Deserialize<MashhadSsoResult<string>>(body)
            ?? new MashhadSsoResult<string> { ErrorCode = -1, ErrorMessage = body };
    }

    public async Task<MashhadSsoResult<MashhadLoginKeyData>> GetLoginKeyAsync(
        string? returnUrl,
        string? state,
        CancellationToken ct = default)
    {
        var primary = NormalizeHashEncoding(_options.HashEncoding);
        var result = await SendLoginKeyAsync(returnUrl, state, primary, ct);
        if (ShouldRetryLoginKeyWithAlternateHash(result, primary))
        {
            var alternate = primary == "upper" ? "lower" : "upper";
            _logger.LogWarning(
                "Mashhad loginKey returned {Code} ({Message}) with HashEncoding={Primary}; retrying with {Alternate}",
                result.ErrorCode,
                result.ErrorMessage,
                primary,
                alternate);
            result = await SendLoginKeyAsync(returnUrl, state, alternate, ct);
        }

        return result;
    }

    private async Task<MashhadSsoResult<MashhadLoginKeyData>> SendLoginKeyAsync(
        string? returnUrl,
        string? state,
        string hashEncoding,
        CancellationToken ct)
    {
        var loginState = string.IsNullOrWhiteSpace(state)
            ? (string.IsNullOrWhiteSpace(_options.LoginState) ? "test" : _options.LoginState.Trim())
            : state.Trim();

        return await SendSignedJsonAsync<MashhadLoginKeyData>(
            "/api/Authentication/loginKey",
            hashEncoding,
            material => new MashhadLoginKeyRequest
            {
                Time = material.RequestTime,
                Hash = material.ApiSecret,
                ClientId = _options.EffectiveClientId,
                State = loginState,
                UserType = _options.LoginUserType,
                DomainId = _options.LoginDomainId,
                ReturnUrl = _options.IncludeReturnUrlInLoginKey && !string.IsNullOrWhiteSpace(returnUrl)
                    ? returnUrl.Trim()
                    : null
            },
            ct);
    }

    /// <summary>
    /// تشخیص 403: همان درخواست loginKey با اعتبار و فرمول هش دلخواه (برای مقایسه با RuleEngine یا آزمودن فرمول‌های دیگر).
    /// </summary>
    public async Task<MashhadSsoResult<MashhadLoginKeyData>> SendLoginKeyProbeAsync(
        string apiName,
        string clientId,
        string secret,
        string requestTime,
        Func<string, string, string> computeApiSecret,
        CancellationToken ct = default,
        bool omitUserTypeAndDomain = false,
        string clientIdKey = "ClientId")
    {
        requestTime = (requestTime ?? "").Trim();
        var hash = computeApiSecret(secret, requestTime);
        var payload = new Dictionary<string, object?>
        {
            ["Time"] = requestTime,
            ["Hash"] = hash,
            [string.IsNullOrWhiteSpace(clientIdKey) ? "ClientId" : clientIdKey] = clientId,
            ["State"] = string.IsNullOrWhiteSpace(_options.LoginState) ? "test" : _options.LoginState.Trim()
        };
        if (!omitUserTypeAndDomain)
        {
            payload["UserType"] = _options.LoginUserType;
            payload["DomainID"] = _options.LoginDomainId;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, Combine("/api/Authentication/loginKey"));
        request.Headers.TryAddWithoutValidation("apiName", apiName);
        request.Headers.TryAddWithoutValidation("requestTime", requestTime);
        request.Headers.TryAddWithoutValidation("apiSecret", hash);
        request.Content = JsonContent.Create(payload, options: MashhadSsoJson.SerializerOptions);

        var client = CreateClient();
        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return Deserialize<MashhadSsoResult<MashhadLoginKeyData>>(body)
            ?? new MashhadSsoResult<MashhadLoginKeyData> { ErrorCode = (int)response.StatusCode, ErrorMessage = body };
    }

    private static bool ShouldRetryLoginKeyWithAlternateHash(MashhadSsoResult<MashhadLoginKeyData> result, string primaryEncoding)
    {
        if (result.IsSuccess)
            return false;

        if (result.ErrorCode != 403)
            return false;

        var message = (result.ErrorMessage ?? "").Trim();
        if (message.Length == 0)
            return true;

        return message.Contains("missmatch", StringComparison.OrdinalIgnoreCase)
            || message.Contains("mismatch", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Client info", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeHashEncoding(string? encoding) =>
        string.Equals(encoding, "upper", StringComparison.OrdinalIgnoreCase) ? "upper" : "lower";

    public async Task<MashhadSsoResult<MashhadAccessTokenData>> GetAccessTokenAsync(
        string refreshToken,
        string username,
        CancellationToken ct = default)
    {
        var result = await SendSignedJsonAsync<MashhadAccessTokenData>(
            "/api/Authentication/getAccessToken",
            _options.HashEncoding,
            _ => new MashhadAccessTokenRequest
            {
                RefreshToken = refreshToken,
                UserName = username,
                ClientId = _options.EffectiveClientId
            },
            ct);
        _logger.LogDebug("Mashhad getAccessToken ErrorCode={Code}", result.ErrorCode);
        return result;
    }

    public async Task<MashhadSsoResult<MashhadUserInfoData>> GetUserInfoAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendSignedJsonAsync<MashhadUserInfoData>(
            "/api/Authentication/getUserInfo",
            _options.HashEncoding,
            _ => new MashhadUserInfoRequest { Token = accessToken },
            ct);
    }

    /// <summary>
    /// بند ۳: getCurrentTime → امضا → POST؛ در صورت انقضای requestTime یک بار با زمان جدید تکرار می‌شود.
    /// </summary>
    private async Task<MashhadSsoResult<T>> SendSignedJsonAsync<T>(
        string relativePath,
        string? hashEncoding,
        Func<MashhadSsoSigningMaterial, object> buildPayload,
        CancellationToken ct)
    {
        var encoding = NormalizeHashEncoding(hashEncoding);
        MashhadSsoResult<T>? last = null;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var time = await GetCurrentTimeAsync(ct);
            if (!time.IsSuccess || string.IsNullOrWhiteSpace(time.Data))
            {
                return new MashhadSsoResult<T>
                {
                    ErrorCode = time.ErrorCode,
                    ErrorMessage = time.ErrorMessage ?? "getCurrentTime ناموفق بود"
                };
            }

            var material = MashhadSsoSigning.CreateMaterial(
                _options.SigningApiName,
                _options.ClientSecret,
                time.Data,
                encoding);

            var payload = buildPayload(material);
            using var request = BuildSignedPost(relativePath, material, payload);
            var client = CreateClient();
            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            last = Deserialize<MashhadSsoResult<T>>(body)
                ?? new MashhadSsoResult<T> { ErrorCode = -1, ErrorMessage = body };

            if (last.IsSuccess || attempt == 1)
                return last;

            if (!MashhadSsoSigning.ShouldRetryWithFreshRequestTime(last.ErrorCode, last.ErrorMessage))
                return last;

            _logger.LogWarning(
                "Mashhad {Path} returned {Code} ({Message}); retrying with fresh getCurrentTime (SSO doc §3 requestTime validity)",
                relativePath,
                last.ErrorCode,
                last.ErrorMessage);
        }

        return last ?? new MashhadSsoResult<T> { ErrorCode = -1, ErrorMessage = "SSO signed request failed" };
    }

    private HttpRequestMessage BuildSignedPost(
        string relativePath,
        MashhadSsoSigningMaterial material,
        object payload)
    {
        if (string.IsNullOrWhiteSpace(material.ApiName))
            throw new InvalidOperationException(
                "Auth:Shimas:ApiName (نام کاربری کاربردی برنامه / apiName) تنظیم نشده است.");

        var request = new HttpRequestMessage(HttpMethod.Post, Combine(relativePath));
        request.Headers.TryAddWithoutValidation("apiName", material.ApiName);
        request.Headers.TryAddWithoutValidation("requestTime", material.RequestTime);
        request.Headers.TryAddWithoutValidation("apiSecret", material.ApiSecret);
        request.Content = JsonContent.Create(payload, options: MashhadSsoJson.SerializerOptions);
        return request;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient(HttpClientName);

    private string Combine(string relativePath)
    {
        var baseUrl = (_options.ApiBaseUrl ?? "").Trim().TrimEnd('/');
        return baseUrl + relativePath;
    }

    private static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, DeserializeOptions);
        }
        catch
        {
            return default;
        }
    }
}

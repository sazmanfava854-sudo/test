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
        var time = await GetCurrentTimeAsync(ct);
        if (!time.IsSuccess || string.IsNullOrWhiteSpace(time.Data))
            return new MashhadSsoResult<MashhadLoginKeyData>
            {
                ErrorCode = time.ErrorCode,
                ErrorMessage = time.ErrorMessage ?? "getCurrentTime ناموفق بود"
            };

        var requestTime = time.Data.Trim();
        var hash = SsoApiSecretHash.ComputeApiSecret(_options.ClientSecret, requestTime, _options.HashEncoding);
        var loginState = string.IsNullOrWhiteSpace(state)
            ? (string.IsNullOrWhiteSpace(_options.LoginState) ? "test" : _options.LoginState.Trim())
            : state.Trim();
        var payload = new MashhadLoginKeyRequest
        {
            Time = requestTime,
            Hash = hash,
            ClientId = _options.EffectiveClientId,
            State = loginState,
            UserType = _options.LoginUserType,
            DomainId = _options.LoginDomainId,
            ReturnUrl = _options.IncludeReturnUrlInLoginKey && !string.IsNullOrWhiteSpace(returnUrl)
                ? returnUrl.Trim()
                : null
        };

        using var request = BuildSignedPost("/api/Authentication/loginKey", requestTime, hash, payload);
        var client = CreateClient();
        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return Deserialize<MashhadSsoResult<MashhadLoginKeyData>>(body)
            ?? new MashhadSsoResult<MashhadLoginKeyData> { ErrorCode = -1, ErrorMessage = body };
    }

    public async Task<MashhadSsoResult<MashhadAccessTokenData>> GetAccessTokenAsync(
        string refreshToken,
        string username,
        CancellationToken ct = default)
    {
        var time = await GetCurrentTimeAsync(ct);
        if (!time.IsSuccess || string.IsNullOrWhiteSpace(time.Data))
            return new MashhadSsoResult<MashhadAccessTokenData>
            {
                ErrorCode = time.ErrorCode,
                ErrorMessage = time.ErrorMessage ?? "getCurrentTime ناموفق بود"
            };

        var requestTime = time.Data.Trim();
        var hash = SsoApiSecretHash.ComputeApiSecret(_options.ClientSecret, requestTime, _options.HashEncoding);
        var payload = new MashhadAccessTokenRequest
        {
            RefreshToken = refreshToken,
            UserName = username,
            ClientId = _options.EffectiveClientId
        };

        using var request = BuildSignedPost("/api/Authentication/getAccessToken", requestTime, hash, payload);
        var client = CreateClient();
        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogDebug("Mashhad getAccessToken HTTP {Status}", (int)response.StatusCode);
        return Deserialize<MashhadSsoResult<MashhadAccessTokenData>>(body)
            ?? new MashhadSsoResult<MashhadAccessTokenData> { ErrorCode = -1, ErrorMessage = body };
    }

    public async Task<MashhadSsoResult<MashhadUserInfoData>> GetUserInfoAsync(string accessToken, CancellationToken ct = default)
    {
        var time = await GetCurrentTimeAsync(ct);
        if (!time.IsSuccess || string.IsNullOrWhiteSpace(time.Data))
            return new MashhadSsoResult<MashhadUserInfoData>
            {
                ErrorCode = time.ErrorCode,
                ErrorMessage = time.ErrorMessage ?? "getCurrentTime ناموفق بود"
            };

        var requestTime = time.Data.Trim();
        var hash = SsoApiSecretHash.ComputeApiSecret(_options.ClientSecret, requestTime, _options.HashEncoding);
        var payload = new MashhadUserInfoRequest { Token = accessToken };

        using var request = BuildSignedPost("/api/Authentication/getUserInfo", requestTime, hash, payload);
        var client = CreateClient();
        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return Deserialize<MashhadSsoResult<MashhadUserInfoData>>(body)
            ?? new MashhadSsoResult<MashhadUserInfoData> { ErrorCode = -1, ErrorMessage = body };
    }

    private HttpRequestMessage BuildSignedPost<TPayload>(
        string relativePath,
        string requestTime,
        string apiSecret,
        TPayload payload)
    {
        var apiName = _options.SigningApiName;
        if (string.IsNullOrWhiteSpace(apiName))
            throw new InvalidOperationException(
                "Auth:Shimas:ApiName (همان SSOUserName در RuleEngine) تنظیم نشده است.");

        var request = new HttpRequestMessage(HttpMethod.Post, Combine(relativePath));
        request.Headers.TryAddWithoutValidation("apiName", apiName);
        request.Headers.TryAddWithoutValidation("requestTime", requestTime);
        request.Headers.TryAddWithoutValidation("apiSecret", apiSecret);
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

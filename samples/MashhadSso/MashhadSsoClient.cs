using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace MashhadSso;

/// <summary>
/// Mashhad SSO — mirrors RuleEngineDocuments.Api.SSO (getCurrentTime, GetloginKey, GetAccessToken, getUserInfo).
/// Doc v1.0.2: headers apiName=SSOUserName, requestTime, apiSecret=SHA256(SSOSecret+requestTime);
/// loginKey body ClientId=SSOClientId; Secret never sent in plain text.
/// </summary>
public sealed class MashhadSsoClient
{
    private readonly MashhadSsoOptions _options;
    private readonly HttpClient _http;

    public MashhadSsoClient(MashhadSsoOptions options, HttpClient? http = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _http = http ?? new HttpClient(new HttpClientHandler { UseProxy = false });
    }

    public async Task<SsoResult<string>> GetCurrentTimeAsync(CancellationToken cancellationToken = default)
    {
        var url = Combine(_options.SSOBaseUrl, "/api/Authentication/getCurrentTime");
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<SsoResult<string>>(request, withAuthHeaders: false, requestTime: null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Starts SSO login: returns loginKey; redirect user to {base}/Authentication/Start/{loginKey}
    /// </summary>
    public async Task<SsoResult<LoginKeyResult>> GetLoginKeyAsync(
        string state = "test",
        int userType = 0,
        int domainId = 0,
        CancellationToken cancellationToken = default)
    {
        var timeResult = await GetCurrentTimeAsync(cancellationToken).ConfigureAwait(false);
        if (timeResult?.Data == null || string.IsNullOrEmpty(timeResult.Data))
            return new SsoResult<LoginKeyResult> { ErrorCode = -1, ErrorMessage = "getCurrentTime failed" };

        var requestTime = timeResult.Data;
        var hash = CryptoHelper.GetHashSha256(_options.SSOSecret + requestTime);

        var body = new LoginKeyRequestBody
        {
            Time = requestTime,
            Hash = hash,
            ClientId = _options.SSOClientId,
            State = state,
            UserType = userType,
            DomainID = domainId
        };

        var url = Combine(_options.SSOBaseUrl, "/api/Authentication/loginKey");
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        return await SendAsync<SsoResult<LoginKeyResult>>(request, withAuthHeaders: true, requestTime, hash, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SsoResult<AccessTokenResult>> GetAccessTokenAsync(
        string refreshToken,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var timeResult = await GetCurrentTimeAsync(cancellationToken).ConfigureAwait(false);
        if (timeResult?.Data == null || string.IsNullOrEmpty(timeResult.Data))
            return new SsoResult<AccessTokenResult> { ErrorCode = -1, ErrorMessage = "getCurrentTime failed" };

        var requestTime = timeResult.Data;
        var hash = CryptoHelper.GetHashSha256(_options.SSOSecret + requestTime);

        var body = new AccessTokenRequestBody
        {
            RefreshToken = refreshToken,
            UserName = userName,
            ClientID = _options.SSOClientId
        };

        var url = Combine(_options.SSOBaseUrl, "/api/Authentication/getAccessToken");
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        return await SendAsync<SsoResult<AccessTokenResult>>(request, withAuthHeaders: true, requestTime, hash, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SsoResult<T>> GetUserInfoAsync<T>(string accessToken, CancellationToken cancellationToken = default)
    {
        var timeResult = await GetCurrentTimeAsync(cancellationToken).ConfigureAwait(false);
        if (timeResult?.Data == null || string.IsNullOrEmpty(timeResult.Data))
            return new SsoResult<T> { ErrorCode = -1, ErrorMessage = "getCurrentTime failed" };

        var requestTime = timeResult.Data;
        var hash = CryptoHelper.GetHashSha256(_options.SSOSecret + requestTime);

        var body = new UserInfoRequestBody { Token = accessToken };
        var url = Combine(_options.SSOBaseUrl, "/api/Authentication/getUserInfo");
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        return await SendAsync<SsoResult<T>>(request, withAuthHeaders: true, requestTime, hash, cancellationToken)
            .ConfigureAwait(false);
    }

    public string GetAuthenticationStartUrl(string loginKey) =>
        Combine(_options.SSOBaseUrl, "/Authentication/Start/" + Uri.EscapeDataString(loginKey));

    private async Task<T> SendAsync<T>(
        HttpRequestMessage request,
        bool withAuthHeaders,
        string? requestTime,
        string? apiSecret = null,
        CancellationToken cancellationToken = default)
    {
        if (withAuthHeaders)
        {
            if (string.IsNullOrEmpty(requestTime) || string.IsNullOrEmpty(apiSecret))
                throw new InvalidOperationException("Auth headers require requestTime and apiSecret");
            request.Headers.TryAddWithoutValidation("apiName", _options.SSOUserName);
            request.Headers.TryAddWithoutValidation("requestTime", requestTime);
            request.Headers.TryAddWithoutValidation("apiSecret", apiSecret);
        }

        try
        {
            using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
                return default!;
            return JsonConvert.DeserializeObject<T>(content)!;
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }

    private static string Combine(string baseUrl, string path)
    {
        baseUrl = baseUrl.TrimEnd('/');
        return baseUrl + path;
    }
}

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class ShimasAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ShimasAuthOptions _options;
    private readonly AppUserRepository _users;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ShimasAuthService> _logger;

    public ShimasAuthService(
        IOptions<ShimasAuthOptions> options,
        AppUserRepository users,
        IHttpClientFactory httpClientFactory,
        ILogger<ShimasAuthService> logger)
    {
        _options = options.Value;
        _users = users;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public ShimasAuthOptions Options => _options;

    public ShimasAuthStatusDto GetStatus() => new()
    {
        Enabled = _options.Enabled,
        SsoReady = _options.SsoReady,
        PreferSsoLogin = _options.PreferSsoLogin,
        LocalLoginAvailable = _options.LocalLoginAvailable,
        LoginPath = "/auth/login",
        CallbackPath = NormalizeCallbackPath(_options.CallbackPath)
    };

    public string ResolveLoginRedirectPath()
    {
        if (_options.PreferSsoLogin)
            return "/auth/login";
        return "/login.html";
    }

    public string BuildExternalLoginUrl(string callbackAbsoluteUrl)
    {
        if (!_options.SsoReady)
            throw new InvalidOperationException("lkey هنوز تنظیم نشده است");

        return QueryHelpers.AddQueryString(_options.LoginUrl, new Dictionary<string, string?>
        {
            [_options.LKeyParameter] = _options.LKey.Trim(),
            [_options.ReturnUrlParameter] = callbackAbsoluteUrl
        });
    }

    public string BuildCallbackAbsoluteUrl(HttpRequest request)
    {
        var path = NormalizeCallbackPath(_options.CallbackPath);
        return $"{request.Scheme}://{request.Host}{path}";
    }

    public async Task<ShimasValidationResult> ValidateAsync(
        string username,
        string refreshToken,
        CancellationToken ct = default)
    {
        var normalizedUsername = (username ?? "").Trim();
        var normalizedToken = (refreshToken ?? "").Trim();

        if (string.IsNullOrWhiteSpace(normalizedUsername))
            return Fail("username خالی است");

        if (normalizedToken.Length < _options.MinRefreshTokenLength)
            return Fail("refresh_token نامعتبر است");

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

    public async Task<AppUserRecord?> ResolveOrCreateUserAsync(
        ShimasUserProfile profile,
        CancellationToken ct = default)
    {
        var username = (profile.Username ?? "").Trim();
        if (username.Length == 0)
            return null;

        var existing = await _users.FindByUsernameAsync(username, ct);
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
                refresh_token = refreshToken
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
        return value.StartsWith('/') ? value : "/" + value;
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

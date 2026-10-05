using Microsoft.Extensions.Configuration;

namespace RayvarzResend.Web.Services;

/// <summary>همان نام‌های RuleEngine — فقط وقتی مقدار Auth:Sso خالی است (جلوگیری از override با Settings سامانهٔ دیگر).</summary>
public static class SsoAuthConfiguration
{
    public static void BindSsoOptions(IConfiguration configuration, SsoAuthOptions options)
    {
        configuration.GetSection(SsoAuthOptions.LegacySectionName).Bind(options);
        configuration.GetSection(SsoAuthOptions.SectionName).Bind(options);
    }

    public static void ApplyMashhadAliases(IConfiguration configuration, SsoAuthOptions options)
    {
        options.ApiName = Coalesce(
            options.ApiName,
            Config(configuration, "SSOUserName"),
            Config(configuration, "ApiName"),
            configuration["Settings:SSOUserName"]);

        options.ClientId = Coalesce(
            options.ClientId,
            options.LKey,
            Config(configuration, "SSOClientId"),
            Config(configuration, "ClientId"),
            configuration["Settings:SSOClientId"]);

        options.ClientSecret = Coalesce(
            options.ClientSecret,
            Config(configuration, "SSOSecret"),
            Config(configuration, "ClientSecret"),
            Config(configuration, "SecretKey"),
            configuration["Settings:SSOSecret"]);

        options.ApiBaseUrl = Coalesce(
            options.ApiBaseUrl,
            Config(configuration, "SSOBaseUrl"),
            configuration["Settings:SSOBaseUrl"]);
    }

    private static string? Config(IConfiguration configuration, string key) =>
        FirstNonEmpty(
            configuration[$"{SsoAuthOptions.SectionName}:{key}"],
            configuration[$"{SsoAuthOptions.LegacySectionName}:{key}"]);

    private static string Coalesce(string? current, params string?[] fallbacks)
    {
        var existing = (current ?? "").Trim();
        if (existing.Length > 0)
            return existing;

        return FirstNonEmpty(fallbacks);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            var text = (value ?? "").Trim();
            if (text.Length > 0)
                return text;
        }

        return "";
    }
}

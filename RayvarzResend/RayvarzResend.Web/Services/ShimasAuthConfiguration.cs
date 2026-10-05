using Microsoft.Extensions.Configuration;

namespace RayvarzResend.Web.Services;

/// <summary>همان نام‌های RuleEngine — فقط وقتی مقدار Shimas خالی است (جلوگیری از override FinancialAssistant با Settings سامانهٔ دیگر).</summary>
public static class ShimasAuthConfiguration
{
    public static void ApplyMashhadAliases(IConfiguration configuration, ShimasAuthOptions options)
    {
        var section = ShimasAuthOptions.SectionName;

        options.ApiName = Coalesce(
            options.ApiName,
            configuration[$"{section}:SSOUserName"],
            configuration[$"{section}:ApiName"],
            configuration["Settings:SSOUserName"]);

        options.ClientId = Coalesce(
            options.ClientId,
            options.LKey,
            configuration[$"{section}:SSOClientId"],
            configuration[$"{section}:ClientId"],
            configuration["Settings:SSOClientId"]);

        options.ClientSecret = Coalesce(
            options.ClientSecret,
            configuration[$"{section}:SSOSecret"],
            configuration[$"{section}:ClientSecret"],
            configuration[$"{section}:SecretKey"],
            configuration["Settings:SSOSecret"]);

        options.ApiBaseUrl = Coalesce(
            options.ApiBaseUrl,
            configuration[$"{section}:SSOBaseUrl"],
            configuration["Settings:SSOBaseUrl"]);
    }

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

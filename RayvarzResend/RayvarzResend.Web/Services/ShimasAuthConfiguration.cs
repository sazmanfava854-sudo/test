using Microsoft.Extensions.Configuration;

namespace RayvarzResend.Web.Services;

/// <summary>همان نام‌های RuleEngine: Settings.SSOUserName / SSOClientId / SSOSecret / SSOBaseUrl</summary>
public static class ShimasAuthConfiguration
{
    public static void ApplyMashhadAliases(IConfiguration configuration, ShimasAuthOptions options)
    {
        options.ApiName = FirstNonEmpty(
            configuration[$"{ShimasAuthOptions.SectionName}:SSOUserName"],
            configuration[$"{ShimasAuthOptions.SectionName}:ApiName"],
            configuration["Settings:SSOUserName"],
            options.ApiName);

        options.ClientId = FirstNonEmpty(
            configuration[$"{ShimasAuthOptions.SectionName}:SSOClientId"],
            configuration[$"{ShimasAuthOptions.SectionName}:ClientId"],
            configuration["Settings:SSOClientId"],
            options.ClientId,
            options.LKey);

        options.ClientSecret = FirstNonEmpty(
            configuration[$"{ShimasAuthOptions.SectionName}:SSOSecret"],
            configuration[$"{ShimasAuthOptions.SectionName}:ClientSecret"],
            configuration[$"{ShimasAuthOptions.SectionName}:SecretKey"],
            configuration["Settings:SSOSecret"],
            options.ClientSecret);

        options.ApiBaseUrl = FirstNonEmpty(
            configuration[$"{ShimasAuthOptions.SectionName}:SSOBaseUrl"],
            configuration["Settings:SSOBaseUrl"],
            options.ApiBaseUrl);
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

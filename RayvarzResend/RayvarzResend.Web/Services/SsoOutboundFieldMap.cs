using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>نقشهٔ ارسال loginKey — جلوگیری از جابه‌جایی apiName و ClientId در appsettings.</summary>
public static class SsoOutboundFieldMap
{
    public static bool LooksLikeLkeyClientId(string? value)
    {
        var t = (value ?? "").Trim();
        if (t.Length != 32)
            return false;

        foreach (var c in t)
        {
            if (!char.IsAsciiLetterOrDigit(c))
                return false;
        }

        return true;
    }

    public static bool LikelyApiNameAndClientIdSwapped(ShimasAuthOptions options)
    {
        var apiName = options.SigningApiName;
        var clientId = options.EffectiveClientId;
        if (string.IsNullOrWhiteSpace(apiName) || string.IsNullOrWhiteSpace(clientId))
            return false;

        return LooksLikeLkeyClientId(apiName) && !LooksLikeLkeyClientId(clientId);
    }

    public static SsoLoginKeyOutboundMap Describe(ShimasAuthOptions options)
    {
        var apiName = options.SigningApiName;
        var bodyClientId = options.EffectiveLoginKeyBodyClientId;
        var swapped = LikelyApiNameAndClientIdSwapped(options);

        var map = new SsoLoginKeyOutboundMap
        {
            HeaderApiName = apiName,
            HeaderApiNameSource = "Auth:Shimas:ApiName یا SSOUserName (نام کاربری API)",
            HeaderApiNameMasked = MaskOrEmpty(apiName),
            BodyClientId = bodyClientId,
            BodyClientIdSource = options.LoginKeyBodyClientIdIsApiName
                ? "همان ApiName (LoginKeyBodyClientIdIsApiName=true)"
                : "Auth:Shimas:ClientId یا LKey (شناسه ۳۲کاراکتری)",
            BodyClientIdMasked = MaskOrEmpty(bodyClientId),
            HeaderRequestTimeSource = "getCurrentTime → هدر requestTime و بدنه Time",
            HeaderApiSecretSource = "SHA256(requestTime + ClientSecret) → هدر apiSecret و بدنه Hash (RuleEngine)",
            LoginKeyBodyClientIdIsApiName = options.LoginKeyBodyClientIdIsApiName,
            LikelyApiNameClientIdSwappedInConfig = swapped
        };

        map.VerdictFa = swapped
            ? "احتمال جابه‌جایی در appsettings: مقدار ۳۲کاراکتری (lkey) در ApiName/SSOUserName و نام کاربری در ClientId است. apiName هدر = FinancialAssistant؛ ClientId بدنه = 53db…9111."
            : (LooksLikeLkeyClientId(apiName) && LooksLikeLkeyClientId(bodyClientId) && string.Equals(apiName, bodyClientId, StringComparison.Ordinal)
                ? "هر دو apiName و ClientId بدنه یکسان و شبیه lkey هستند — اگر SSO نام کاربری جدا داده، ApiName را FinancialAssistant بگذارید."
                : "نقشهٔ فعلی: apiName در هدر، ClientId در بدنه، Secret فقط برای هش (هرگز در URL).");

        return map;
    }

    private static string MaskOrEmpty(string value)
    {
        var t = (value ?? "").Trim();
        return t.Length == 0 ? "" : SsoCredentialMask.MaskId(t);
    }
}

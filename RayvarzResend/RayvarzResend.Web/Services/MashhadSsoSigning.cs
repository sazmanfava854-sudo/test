namespace RayvarzResend.Web.Services;

/// <summary>
/// منطق بند ۳ سند «اتصال فنی به درگاه احراز هویت» (صفحه ۲۰):
/// ۱) apiName = نام کاربری کاربردی برنامه
/// ۲) requestTime = خروجی getCurrentTime (تنها سرویس بدون این هدرها)
/// ۳) apiSecret = SHA256(SecretKey + requestTime) — SecretKey همان ClientSecret ثبت‌شده
/// برای loginKey فیلدهای بدنه Time و Hash همان requestTime و همان مقدار apiSecret هستند.
/// </summary>
public static class MashhadSsoSigning
{
    public static MashhadSsoSigningMaterial CreateMaterial(
        string apiName,
        string secretKey,
        string requestTime,
        string? hashEncoding)
    {
        var time = (requestTime ?? "").Trim();
        var apiSecret = SsoApiSecretHash.ComputeApiSecret(secretKey, time, hashEncoding);
        return new MashhadSsoSigningMaterial(
            (apiName ?? "").Trim(),
            time,
            apiSecret);
    }

    /// <summary>
    /// وقتی requestTime از پنجرهٔ اعتبار SSO خارج شده باشد (سند: خطای مربوط به SecretKey / انقضا).
    /// </summary>
    public static bool ShouldRetryWithFreshRequestTime(int errorCode, string? errorMessage)
    {
        if (errorCode == 0)
            return false;

        var m = (errorMessage ?? "").Trim();
        if (m.Length == 0)
            return false;

        if (m.Contains("expire", StringComparison.OrdinalIgnoreCase)
            || m.Contains("expir", StringComparison.OrdinalIgnoreCase)
            || m.Contains("اکسپایر", StringComparison.OrdinalIgnoreCase)
            || m.Contains("منقضی", StringComparison.OrdinalIgnoreCase))
            return true;

        if (m.Contains("requestTime", StringComparison.OrdinalIgnoreCase)
            || m.Contains("request time", StringComparison.OrdinalIgnoreCase))
            return true;

        return errorCode != 403
            && m.Contains("time", StringComparison.OrdinalIgnoreCase)
            && (m.Contains("invalid", StringComparison.OrdinalIgnoreCase)
                || m.Contains("نامعتبر", StringComparison.OrdinalIgnoreCase));
    }
}

public readonly record struct MashhadSsoSigningMaterial(string ApiName, string RequestTime, string ApiSecret);

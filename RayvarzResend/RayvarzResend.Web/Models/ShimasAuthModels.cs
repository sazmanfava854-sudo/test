namespace RayvarzResend.Web.Models;

public sealed class ShimasUserProfile
{
    public string Username { get; set; } = "";
    public string Domain { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string Position { get; set; } = "";
    public string District { get; set; } = "";
}

public sealed class ShimasValidationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public bool UsedRemoteApi { get; set; }
    public ShimasUserProfile Profile { get; set; } = new();
}

public sealed class SsoLoginKeyProbeResult
{
    public string Variant { get; set; } = "";
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class SsoLoginKeyDiagnostics
{
    public string ApiBaseUrl { get; set; } = "";
    public string ApiName { get; set; } = "";
    public string ClientIdMasked { get; set; } = "";
    public int ClientIdLength { get; set; }
    public int ClientSecretLength { get; set; }
    public bool UseLoginKeyOnRedirect { get; set; }
    public string ReturnUrlRegisteredInPortal { get; set; } = "";
    public bool GetCurrentTimeOk { get; set; }
    public bool LoginKeyOk { get; set; }
    public int? LoginKeyErrorCode { get; set; }
    public string? LoginKeyErrorMessage { get; set; }
    public string? StartUrlSample { get; set; }
    public string? Error { get; set; }

    public string Verdict =>
        LoginKeyOk
            ? "OK — SSO loginKey می‌دهد؛ با UseLoginKeyOnRedirect=true کاربر به Start/{loginKey} می‌رود و به «برگشت آدرس» ثبت‌شده برمی‌گردد."
            : Error ?? "نامشخص";
}

public sealed class SsoSigningPreviewDto
{
    public string HashFormula { get; set; } = "SHA256(ClientSecret + requestTime) hex";
    public string HashEncoding { get; set; } = "lower";
    public int ClientSecretLength { get; set; }
    public SsoSigningHeaderPreview Headers { get; set; } = new();
    public SsoSigningBodyPreview Body { get; set; } = new();
    public string NoteFa { get; set; } = "";
}

public sealed class SsoSigningHeaderPreview
{
    public string ApiName { get; set; } = "";
    public string RequestTime { get; set; } = "";
    public string ApiSecret { get; set; } = "";
}

public sealed class SsoSigningBodyPreview
{
    public string Time { get; set; } = "";
    public string Hash { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string State { get; set; } = "";
    public int UserType { get; set; }
    public int DomainId { get; set; }
}

public sealed class SsoLoginKeyOutboundMap
{
    public string HeaderApiNameSource { get; set; } = "";
    public string? HeaderApiName { get; set; }
    public string? HeaderApiNameMasked { get; set; }
    public string HeaderRequestTimeSource { get; set; } = "";
    public string HeaderApiSecretSource { get; set; } = "";
    public string BodyClientIdSource { get; set; } = "";
    public string? BodyClientId { get; set; }
    public string? BodyClientIdMasked { get; set; }
    public bool LoginKeyBodyClientIdIsApiName { get; set; }
    public bool LikelyApiNameClientIdSwappedInConfig { get; set; }
    public string VerdictFa { get; set; } = "";
}

public sealed class ShimasAuthStatusDto
{
    public bool Enabled { get; set; }
    public bool SsoReady { get; set; }
    public bool PreferSsoLogin { get; set; }
    public bool LocalLoginAvailable { get; set; }
    /// <summary>صفحه login.html روی host عمومی برای ادمین (بدون ریدایرکت اجباری به SSO).</summary>
    public bool AllowAdminLocalLoginOnPublicHost { get; set; }
    /// <summary>ورود دوگانه: فرم محلی (دامین/رمز AppUser) + دکمه SSO.</summary>
    public bool AllowHybridLocalLogin { get; set; }
    public string LoginPath { get; set; } = "/auth/login";
    public string PostLoginDefaultPath { get; set; } = "/";
    public string CallbackPath { get; set; } = "/auth/callback";
    /// <summary>آدرس callback ثبت‌شده در سامزان — برای بررسی پیکربندی.</summary>
    public string? RegisteredCallbackUrl { get; set; }
    /// <summary>همان مقدار «برگشت آدرس» برای ثبت در پورتال SSO (جدول ۱ ردیف ۴).</summary>
    public string? SsoReturnUrlForPortal { get; set; }
    /// <summary>برای خطای 403 — همان SSOUserName (apiName).</summary>
    public string? SigningApiName { get; set; }
    public string? ClientIdHint { get; set; }
    public bool ClientSecretLooksShort { get; set; }
    public SsoLoginKeyOutboundMap? SsoOutboundMap { get; set; }
}

public sealed class ShimasCallbackPayload
{
    public string Username { get; set; } = "";
    public string Domain { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string State { get; set; } = "";
}

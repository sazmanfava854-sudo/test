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

public sealed class ShimasAuthStatusDto
{
    public bool Enabled { get; set; }
    public bool SsoReady { get; set; }
    public bool PreferSsoLogin { get; set; }
    public bool LocalLoginAvailable { get; set; }
    /// <summary>صفحه login.html روی host عمومی برای ادمین (بدون ریدایرکت اجباری به SSO).</summary>
    public bool AllowAdminLocalLoginOnPublicHost { get; set; }
    public string LoginPath { get; set; } = "/auth/login";
    public string CallbackPath { get; set; } = "/auth/callback";
    /// <summary>آدرس callback ثبت‌شده در سامزان — برای بررسی پیکربندی.</summary>
    public string? RegisteredCallbackUrl { get; set; }
    /// <summary>همان مقدار «برگشت آدرس» برای ثبت در پورتال SSO (جدول ۱ ردیف ۴).</summary>
    public string? SsoReturnUrlForPortal { get; set; }
    /// <summary>برای خطای 403 — همان SSOUserName (apiName).</summary>
    public string? SigningApiName { get; set; }
    public string? ClientIdHint { get; set; }
    public bool ClientSecretLooksShort { get; set; }
}

public sealed class ShimasCallbackPayload
{
    public string Username { get; set; } = "";
    public string Domain { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string State { get; set; } = "";
}

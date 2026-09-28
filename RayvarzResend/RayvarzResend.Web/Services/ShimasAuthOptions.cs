namespace RayvarzResend.Web.Services;

public sealed class ShimasAuthOptions
{
    public const string SectionName = "Auth:Shimas";

    public bool Enabled { get; set; }
    /// <summary>آدرس عمومی سایت — برای callback سامزان/شیماس روی سرور پشت IIS یا IP:Port.</summary>
    public string PublicBaseUrl { get; set; } = "";
    /// <summary>همان «برگشت آدرس» جدول ۱ SSO — اگر پر باشد عیناً در returnUrl/loginKey استفاده می‌شود.</summary>
    public string SsoRegisteredReturnUrl { get; set; } = "";
    /// <summary>مسیر IIS مثل /RayvarzResend — اگر خالی، از PathBase درخواست گرفته می‌شود.</summary>
    public string ApplicationPath { get; set; } = "";
    /// <summary>بعد از ورود موفق SSO به کدام مسیر داخلی برود (صفحهٔ اصلی با تب‌ها).</summary>
    public string PostLoginDefaultPath { get; set; } = "/";
    public string LoginUrl { get; set; } = "https://login.mashhad.ir/Authentication/Login.aspx";
    /// <summary>طبق سند SSO: https://login.mashhad.ir/Authentication/Start/{loginKey}</summary>
    public string LoginStartUrlTemplate { get; set; } = "https://login.mashhad.ir/Authentication/Start/{loginKey}";
    /// <summary>پایه API احراز هویت (همان SSOBaseUrl در RuleEngine) — مثلاً https://login.mashhad.ir</summary>
    public string ApiBaseUrl { get; set; } = "https://login.mashhad.ir";
    /// <summary>نام کاربری ثبت‌شده در SSO (جدول ۱ ردیف ۲) — هدر apiName؛ **نه** ClientId/lkey.</summary>
    public string ApiName { get; set; } = "";
    public string LoginKeyParameter { get; set; } = "loginKey";
    /// <summary>همان State در RuleEngine (معمولاً test). SSO همان مقدار را در callback برمی‌گرداند.</summary>
    public string LoginState { get; set; } = "test";
    /// <summary>lower = SHA256 hex lowercase (پیش‌فرض RuleEngine) — upper در صورت خطای Client info.</summary>
    public string HashEncoding { get; set; } = "lower";
    /// <summary>اگر loginKey خطا بدهد به Login.aspx برود (همان روال قبلی FinancialAssistant روی IIS).</summary>
    public bool AllowLegacyLoginUrlWithoutLoginKey { get; set; } = true;
    /// <summary>RuleEngine در loginKey فیلد ReturnUrl نمی‌فرستد؛ برگشت از «برگشت آدرس» ثبت SSO است.</summary>
    public bool IncludeReturnUrlInLoginKey { get; set; }
    /// <summary>true = loginKey API؛ false = Login.aspx?lkey+returnUrl (پیش‌فرض — همان نسخهٔ پایدار قبلی).</summary>
    public bool UseLoginKeyOnRedirect { get; set; }
    public int LoginUserType { get; set; }
    public int LoginDomainId { get; set; }
    /// <summary>شناسه سامانه در لاگین یکپارچه (همان lkey در login.mashhad.ir).</summary>
    public string ClientId { get; set; } = "";
    /// <summary>رمز سامانه — فقط سمت سرور برای اعتبارسنجی توکن؛ در URL مرورگر نمی‌رود.</summary>
    public string ClientSecret { get; set; } = "";
    public string LKey { get; set; } = "";
    public string CallbackPath { get; set; } = "/auth/callback";
    public string ReturnUrlParameter { get; set; } = "returnUrl";
    public string LKeyParameter { get; set; } = "lkey";
    public string ClientIdParameter { get; set; } = "client_id";
    public string ValidateTokenUrl { get; set; } = "";
    public string UserProfileUrl { get; set; } = "";
    public bool AutoProvisionUsers { get; set; } = true;
    public bool AllowLocalLoginFallback { get; set; } = true;
    /// <summary>روی آدرس عمومی (مثلاً city.mashhad.ir) ورود محلی فقط برای کاربران IsAdmin.</summary>
    public bool AllowAdminLocalLoginOnPublicHost { get; set; } = true;
    public int MinRefreshTokenLength { get; set; } = 3;

    public string EffectiveClientId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ClientId))
                return ClientId.Trim();
            return (LKey ?? "").Trim();
        }
    }

    /// <summary>هدر apiName — باید SSOUserName باشد، نه lkey.</summary>
    public string SigningApiName => (ApiName ?? "").Trim();

    public string EffectiveApiName =>
        string.IsNullOrWhiteSpace(SigningApiName) ? EffectiveClientId : SigningApiName;

    public bool HasLKey => !string.IsNullOrWhiteSpace(EffectiveClientId);
    public bool HasClientSecret => !string.IsNullOrWhiteSpace(ClientSecret);
    public bool UseMashhadAuthenticationApi =>
        !string.IsNullOrWhiteSpace(ApiBaseUrl)
        && !string.IsNullOrWhiteSpace(SigningApiName)
        && HasClientSecret;

    public bool SsoReady => Enabled && HasLKey && (!UseMashhadAuthenticationApi || HasClientSecret);
    public bool LocalLoginAvailable => !Enabled || (!SsoReady && AllowLocalLoginFallback);
    public bool PreferSsoLogin => Enabled && (SsoReady || !AllowLocalLoginFallback);

    /// <summary>localhost برای تست نسخه غیرپابلیش — SSO فقط روی آدرس عمومی سرور.</summary>
    public static bool IsLoopbackHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        var name = host.Trim();
        if (name.StartsWith('['))
        {
            var end = name.IndexOf(']');
            name = end > 1 ? name[1..end] : name;
        }
        else
        {
            var colon = name.LastIndexOf(':');
            if (colon > 0 && name.Count(static c => c == ':') == 1)
                name = name[..colon];
        }

        return name.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || name == "127.0.0.1"
            || name == "::1";
    }

    public bool PreferSsoLoginForHost(string? host) =>
        PreferSsoLogin && !IsLoopbackHost(host);

    public bool LocalLoginAvailableForHost(string? host) =>
        LocalLoginAvailable || IsLoopbackHost(host);
}

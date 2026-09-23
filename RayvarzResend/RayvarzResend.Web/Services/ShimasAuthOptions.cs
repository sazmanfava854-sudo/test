namespace RayvarzResend.Web.Services;

public sealed class ShimasAuthOptions
{
    public const string SectionName = "Auth:Shimas";

    public bool Enabled { get; set; }
    /// <summary>آدرس عمومی سایت — برای callback سامزان/شیماس روی سرور پشت IIS یا IP:Port.</summary>
    public string PublicBaseUrl { get; set; } = "";
    public string LoginUrl { get; set; } = "https://login.mashhad.ir/Authentication/Login.aspx";
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

    public bool HasLKey => !string.IsNullOrWhiteSpace(EffectiveClientId);
    public bool HasClientSecret => !string.IsNullOrWhiteSpace(ClientSecret);
    public bool SsoReady => Enabled && HasLKey;
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

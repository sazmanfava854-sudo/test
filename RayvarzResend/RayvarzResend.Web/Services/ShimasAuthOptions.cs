namespace RayvarzResend.Web.Services;

public sealed class ShimasAuthOptions
{
    public const string SectionName = "Auth:Shimas";

    public bool Enabled { get; set; }
    public string LoginUrl { get; set; } = "https://login.mashhad.ir/Authentication/Login.aspx";
    public string LKey { get; set; } = "";
    public string CallbackPath { get; set; } = "/auth/callback";
    public string ReturnUrlParameter { get; set; } = "returnUrl";
    public string LKeyParameter { get; set; } = "lkey";
    public string ValidateTokenUrl { get; set; } = "";
    public string UserProfileUrl { get; set; } = "";
    public bool AutoProvisionUsers { get; set; } = true;
    public bool AllowLocalLoginFallback { get; set; } = true;
    public int MinRefreshTokenLength { get; set; } = 3;

    public bool HasLKey => !string.IsNullOrWhiteSpace(LKey);
    public bool SsoReady => Enabled && HasLKey;
    public bool LocalLoginAvailable => !Enabled || (!SsoReady && AllowLocalLoginFallback);
    public bool PreferSsoLogin => Enabled && (SsoReady || !AllowLocalLoginFallback);
}

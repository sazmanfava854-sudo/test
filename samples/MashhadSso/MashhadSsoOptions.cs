namespace MashhadSso;

/// <summary>
/// Same fields as RuleEngineDocuments Settings (SSOBaseUrl, SSOUserName, SSOClientId, SSOSecret).
/// Use in FinancialAssistant appsettings and RayvarzResend configuration.
/// </summary>
public sealed class MashhadSsoOptions
{
    public const string SectionName = "MashhadSso";

    public string SSOBaseUrl { get; set; } = "https://login.mashhad.ir";
    public string SSOUserName { get; set; } = "";
    public string SSOClientId { get; set; } = "";
    public string SSOSecret { get; set; } = "";
}

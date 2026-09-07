namespace RayvarzResend.Web.Services;

public sealed class BankInquiryConfirmOptions
{
    public const string SectionName = "BankInquiryConfirm";

    public bool DryRun { get; set; }

    public string ServiceUrl { get; set; } =
        "https://epayws.mashhad.ir/api/Proxy/epay_EstelamOnLineBank";

    public string UserName { get; set; } = "FinancialAssistant";

    public string Password { get; set; } = "";

    public int BankCode { get; set; } = 18;

    public bool AllowInvalidSsl { get; set; }

    public bool UseSystemProxy { get; set; }

    public string? ProxyUrl { get; set; }

    public string? FallbackServiceUrl { get; set; }

    public int RetryCount { get; set; } = 2;

    public int RetryDelayMs { get; set; } = 1000;

    public int TimeoutSeconds { get; set; } = 60;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ServiceUrl)
        && !string.IsNullOrWhiteSpace(UserName)
        && !string.IsNullOrWhiteSpace(Password);
}

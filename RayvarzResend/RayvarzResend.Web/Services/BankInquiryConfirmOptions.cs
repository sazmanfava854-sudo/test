namespace RayvarzResend.Web.Services;

public sealed class BankInquiryConfirmOptions
{
    public const string SectionName = "BankInquiryConfirm";

    public bool DryRun { get; set; }

    public string ServiceUrl { get; set; } =
        "https://epayws.mashhad.ir/api/Proxy/epay_EstelamOnLineBank";

    public string FicheLookupServiceUrl { get; set; } =
        "https://epayws.mashhad.ir/api/Proxy/epay_FindEpayFichesByBillIdPayId";

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

    /// <summary>epayws فقط روی HTTPS پاسخ می‌دهد؛ http:// به 404 IIS می‌رسد. (loopback برای تست محلی مستثنی است)</summary>
    public static string EnforceHttps(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "";

        var trimmed = url.Trim();
        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && uri.IsLoopback)
            return trimmed;

        return "https://" + trimmed["http://".Length..];
    }

    public string EffectiveServiceUrl => EnforceHttps(ServiceUrl);

    public string EffectiveFicheLookupServiceUrl => EnforceHttps(FicheLookupServiceUrl);

    public string? EffectiveFallbackServiceUrl =>
        string.IsNullOrWhiteSpace(FallbackServiceUrl) ? null : EnforceHttps(FallbackServiceUrl);

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ServiceUrl)
        && !string.IsNullOrWhiteSpace(FicheLookupServiceUrl)
        && !string.IsNullOrWhiteSpace(UserName)
        && !string.IsNullOrWhiteSpace(Password);
}

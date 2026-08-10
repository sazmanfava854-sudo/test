namespace RayvarzResend.Web.Validation;

public enum RayvarzValidationSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>یک مورد اعتبارسنجی — مطابق مستند RayReceiveIncmVchr و قواعد مالی Resend.</summary>
public sealed class RayvarzValidationIssue
{
    public string Code { get; init; } = "";
    public string Field { get; init; } = "";
    /// <summary>SetHeaderDoc | AddDocItem | AddIncm | SaveDocument | PreSend | Compatibility</summary>
    public string Operation { get; init; } = "";
    public RayvarzValidationSeverity Severity { get; init; }
    public bool Blocking { get; init; }
    public string Message { get; init; } = "";
}

public sealed class RayvarzValidationResult
{
    public IReadOnlyList<RayvarzValidationIssue> Issues { get; init; } = Array.Empty<RayvarzValidationIssue>();

    public bool CanSend => Issues.All(i => !i.Blocking);

    public IReadOnlyList<RayvarzValidationIssue> BlockingIssues =>
        Issues.Where(i => i.Blocking).ToList();

    public IReadOnlyList<RayvarzValidationIssue> Warnings =>
        Issues.Where(i => !i.Blocking && i.Severity == RayvarzValidationSeverity.Warning).ToList();

    public static RayvarzValidationResult Merge(params RayvarzValidationResult[] results)
    {
        var all = results.SelectMany(r => r.Issues).ToList();
        return new RayvarzValidationResult { Issues = all };
    }
}

public sealed class RayvarzValidationInput
{
    public RayvarzResend.Web.Models.FicheHeaderDto Fiche { get; init; } = new();
    public string? SoapXml { get; init; }
    public int Branch { get; init; }
    public int Fund { get; init; }
    public string? DocDate { get; init; }
    public string? ActDate { get; init; }
    public string? DueDate { get; init; }
    public IReadOnlyList<string> CompatibilityWarnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PreSoapRuleErrors { get; init; } = Array.Empty<string>();
    public bool ExistsInRayvarz { get; init; }
}

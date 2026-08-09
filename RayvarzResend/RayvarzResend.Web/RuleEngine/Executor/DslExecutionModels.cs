using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

public sealed class DslExecutionContext
{
    public FicheHeaderDto Fiche { get; init; } = new();
    public int Branch { get; init; }
    public int Fund { get; init; }
    public string? DocDate { get; init; }
    public string? ActDate { get; init; }
    public string? DueDate { get; init; }
    /// <summary>حالت ارزیابی قوانین: خطوط VB غیرقابل‌parse و operation ناشناخته soft-skip می‌شوند.</summary>
    public bool DryRun { get; init; } = true;
    public bool BuildSoap { get; init; }
    public bool AllowLegacyFallback { get; init; } = true;
    public Dictionary<string, object?> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<IncmRowDto> Rows { get; } = new();
    public string? LastReturnValue { get; set; }
    public string DispatchedFunction { get; set; } = "";
    /// <summary>ترتیب توابع فراخوانی‌شده از Run (اعمال‌شده روی این فیش).</summary>
    public List<string> InvokedFunctions { get; } = new();
    /// <summary>توابعی که به‌خاطر نوع فیش اعمال نشدند (نه Unsupported).</summary>
    public List<string> SkippedNotApplicable { get; } = new();
    public List<string> DeferredRuleLines { get; } = new();
}

public sealed class DslExecutionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string DispatchedFunction { get; init; } = "";
    public IReadOnlyList<IncmRowDto> Rows { get; init; } = Array.Empty<IncmRowDto>();
    public decimal RowSum { get; init; }
    public IReadOnlyList<string> Trace { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AppliedFunctions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SkippedNotApplicable { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PreSoapRuleErrors { get; init; } = Array.Empty<string>();
}

public sealed class DslValidationResult
{
    public bool Success { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UnknownOperations { get; init; } = Array.Empty<string>();
}

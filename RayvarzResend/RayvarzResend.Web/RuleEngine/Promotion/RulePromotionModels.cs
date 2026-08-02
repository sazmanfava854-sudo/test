using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Promotion;

public sealed class RulePromotionRunResult
{
    public bool AnyAction { get; init; }
    public bool Promoted { get; init; }
    public long? CandidateId { get; init; }
    public long? SnapshotId { get; init; }
    public string? Message { get; init; }
    /// <summary>مرحله‌ای که متوقف شد: Parse / Validate / GoldenDryRun / Stability / Hash / Promote</summary>
    public string? FailedStage { get; init; }
    public IReadOnlyList<string> Steps { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UnknownOperations { get; init; } = Array.Empty<string>();
    public IReadOnlyList<object> GoldenFailures { get; init; } = Array.Empty<object>();
    public IReadOnlyList<object> CandidateStatuses { get; init; } = Array.Empty<object>();
}

public sealed class RulePromotionStatus
{
    public int NidMember { get; init; }
    public string ActiveEngine { get; init; } = "Legacy";
    public int ActiveDslVersion { get; init; }
    public long? ActiveSnapshotId { get; init; }
    public int ConsecutiveDynamicFailures { get; init; }
    public DateTime? CircuitBreakerOpenUntilUtc { get; init; }
    public bool CircuitBreakerOpen { get; init; }
    public bool EnableAutoPromote { get; init; }
    public int StabilityHours { get; init; }
    public IReadOnlyList<object> Candidates { get; init; } = Array.Empty<object>();
    public IReadOnlyList<object> RejectedCandidates { get; init; } = Array.Empty<object>();
    public IReadOnlyList<RulePromotionLogRow> RecentLogs { get; init; } = Array.Empty<RulePromotionLogRow>();
}

using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Promotion;

public sealed class RulePromotionRunResult
{
    public bool AnyAction { get; init; }
    public bool Promoted { get; init; }
    public long? CandidateId { get; init; }
    public long? SnapshotId { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Steps { get; init; } = Array.Empty<string>();
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
    public IReadOnlyList<RulePromotionLogRow> RecentLogs { get; init; } = Array.Empty<RulePromotionLogRow>();
}

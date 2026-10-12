namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class MemberHistoryHead
{
    public long NidHistory { get; set; }
    public int NidMember { get; set; }
    public string XmlBody { get; set; } = "";
    public DateTime? ModifyUtc { get; set; }
    public string? Modifyer { get; set; }
    public string? ModifyDesc { get; set; }
}

/// <summary>ردیف dbo.RuleSyncState — نام ستون‌ها مطابق اسکیمای RayvarzRuleEngine موجود.</summary>
public sealed class RuleSyncStateRow
{
    public int NidMember { get; set; }
    public int NidClass { get; set; } = 360;
    public long? LastSeenNidHistory { get; set; }
    public DateTime? LastSeenModifyAt { get; set; }
    public long? LastStableNidHistory { get; set; }
    public DateTime? LastStableModifyAt { get; set; }
    public string LastStableXmlHash { get; set; } = "";
    public int? ActiveDslVersion { get; set; }
    public string ActiveEngine { get; set; } = "Legacy";
    public long? ActiveSnapshotId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int ConsecutiveDynamicFailures { get; set; }
    public DateTime? CircuitBreakerOpenUntilUtc { get; set; }
}

public static class RuleCandidateStatuses
{
    public const string PendingStability = "PendingStability";
    public const string StableEligible = "StableEligible";
    public const string Promoted = "Promoted";
    public const string Rejected = "Rejected";
}

public static class RulePromotionActions
{
    public const string Promoted = "Promoted";
    public const string Rejected = "Rejected";
}

/// <summary>ردیف dbo.RuleCandidate.</summary>
public sealed class RuleCandidateRow
{
    public long CandidateId { get; set; }
    public int NidMember { get; set; }
    public long? SourceNidHistory { get; set; }
    public DateTime? SourceModifyAt { get; set; }
    public string CanonicalXmlHash { get; set; } = "";
    public string XmlBody { get; set; } = "";
    public string? Modifyer { get; set; }
    public string? ModifyDesc { get; set; }
    public string Status { get; set; } = RuleCandidateStatuses.PendingStability;
    public string? RejectReason { get; set; }
    public DateTime? StableEligibleAtUtc { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime? LastCheckedAtUtc { get; set; }
}

public sealed class RuleSyncCycleResult
{
    public bool Ok { get; set; } = true;
    public string Summary { get; set; } = "";
    public string? MemberHash { get; set; }
    public string? ActiveHash { get; set; }
    public bool MemberHashChanged { get; set; }
    public long? LatestNidHistory { get; set; }
    public bool HistoryAdvanced { get; set; }
    public long? CandidateId { get; set; }
    public bool Promoted { get; set; }
    public string? Error { get; set; }
}

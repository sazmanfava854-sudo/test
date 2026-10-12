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

/// <summary>ردیف dbo.RuleSyncState (database/01_RayvarzRuleEngine_Schema.sql + 03_Phase4).</summary>
public sealed class RuleSyncStateRow
{
    public int NidMember { get; set; }
    public int NidClass { get; set; } = 360;
    public long? LastSeenNidHistory { get; set; }
    public DateTime? LastSeenModifyAt { get; set; }
    public long? LastStableNidHistory { get; set; }
    public DateTime? LastStableModifyAt { get; set; }
    public string LastStableXmlHash { get; set; } = "";
    public int ActiveDslVersion { get; set; }
    public string ActiveEngine { get; set; } = "Legacy";
    public long? ActiveSnapshotId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int ConsecutiveDynamicFailures { get; set; }
    public DateTime? CircuitBreakerOpenUntilUtc { get; set; }
}

/// <summary>مقادیر RuleCandidate.Status — سازگار با ردیف‌های نسخه قبلی (Detected/Parsed/Validated/Stable/Promoted/Rejected).</summary>
public static class RuleCandidateStatuses
{
    public const string Detected = "Detected";
    public const string Parsed = "Parsed";
    public const string Validated = "Validated";
    public const string Stable = "Stable";
    public const string Promoted = "Promoted";
    public const string Rejected = "Rejected";

    /// <summary>در انتظار پایان دوره ثبات.</summary>
    public static readonly string[] Pending = [Detected, Parsed, Validated];

    public static bool IsPending(string? status) =>
        Pending.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
}

public static class RulePromotionActions
{
    public const string Baseline = "Baseline";
    public const string Stable = "Stable";
    public const string Promoted = "Promoted";
    public const string Rejected = "Rejected";
    public const string Superseded = "Superseded";
}

/// <summary>ردیف dbo.RuleCandidate.</summary>
public sealed class RuleCandidateRow
{
    public long CandidateId { get; set; }
    public int NidMember { get; set; }
    public long SourceNidHistory { get; set; }
    public DateTime SourceModifyAt { get; set; }
    public string CanonicalXmlHash { get; set; } = "";
    public string XmlBody { get; set; } = "";
    public string? Modifyer { get; set; }
    public string? ModifyDesc { get; set; }
    public string Status { get; set; } = RuleCandidateStatuses.Detected;
    public string? RejectReason { get; set; }
    public DateTime StableEligibleAtUtc { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastCheckedAtUtc { get; set; }
}

public sealed class RuleSyncCycleResult
{
    public bool Ok { get; set; } = true;
    public string Summary { get; set; } = "";
    public string? MemberHash { get; set; }
    public string? StableHash { get; set; }
    public long? LatestNidHistory { get; set; }
    public long? CandidateId { get; set; }
    public bool Accepted { get; set; }
    public bool Promoted { get; set; }
    public string? Error { get; set; }
}

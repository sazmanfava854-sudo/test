namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleSyncStateRow
{
    public int NidMember { get; set; }
    public string ActiveEngine { get; set; } = "Legacy";
    public string ActiveXmlBodyHash { get; set; } = "";
    public int ActiveMemberVersion { get; set; }
    public long? LastSeenNidHistory { get; set; }
    public DateTime? LastHistoryModifyUtc { get; set; }
    public string LastMemberXmlBodyHash { get; set; } = "";
    public DateTime? LastCheckAtUtc { get; set; }
    public DateTime? LastChangeDetectedAtUtc { get; set; }
    public DateTime? LastPromotedAtUtc { get; set; }
    public string? LastError { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class MemberHistoryHead
{
    public long NidHistory { get; set; }
    public int NidMember { get; set; }
    public string XmlBody { get; set; } = "";
    public DateTime? ModifyUtc { get; set; }
    public string? Modifyer { get; set; }
    public string? ModifyDesc { get; set; }
}

public enum RuleCandidateState
{
    Detected,
    Parsing,
    Parsed,
    Validated,
    Stable,
    PromoteReady,
    Promoted,
    Rejected
}

public sealed class RuleCandidateRow
{
    public Guid Id { get; set; }
    public int NidMember { get; set; }
    public long? NidHistory { get; set; }
    public string XmlBodyHash { get; set; } = "";
    public int MemberVersion { get; set; }
    public RuleCandidateState State { get; set; }
    public DateTime DetectedAtUtc { get; set; }
    public DateTime? StableSinceUtc { get; set; }
    public bool ParseOk { get; set; }
    public bool ValidationOk { get; set; }
    public bool DryRunOk { get; set; }
    public string? Notes { get; set; }
}

public sealed class RuleSyncCycleResult
{
    public bool Ok { get; set; }
    public string Summary { get; set; } = "";
    public string? MemberHash { get; set; }
    public string? ActiveHash { get; set; }
    public bool MemberHashChanged { get; set; }
    public long? LatestNidHistory { get; set; }
    public bool HistoryAdvanced { get; set; }
    public Guid? CandidateId { get; set; }
    public bool Promoted { get; set; }
    public string? Error { get; set; }
}

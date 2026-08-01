namespace RayvarzResend.Web.RuleEngine.Store;

public sealed class RuleSyncStateRow
{
    public int NidMember { get; init; }
    public int NidClass { get; init; } = 360;
    public long? LastSeenNidHistory { get; init; }
    public DateTime? LastSeenModifyAt { get; init; }
    public long? LastStableNidHistory { get; init; }
    public DateTime? LastStableModifyAt { get; init; }
    public string? LastStableXmlHash { get; init; }
    public int ActiveDslVersion { get; init; }
    public string ActiveEngine { get; init; } = "Legacy";
    public long? ActiveSnapshotId { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}

public sealed class RuleGoldenFicheRow
{
    public int GoldenFicheId { get; init; }
    public string Name { get; init; } = "";
    public string FicheNo { get; init; } = "";
    public Guid NidFiche { get; init; }
    public int NidMember { get; init; }
    public string Scenario { get; init; } = "";
    public int ExpectedRowCount { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
}

public sealed class RuleGoldenExpectedRow
{
    public int GoldenFicheId { get; init; }
    public int IncmRow { get; init; }
    public int IncmNo { get; init; }
    public decimal ExpectedVal { get; init; }
    public string? IncmRowDsc { get; init; }
    public int? ExpectedBranch { get; init; }
    public int? ExpectedBank { get; init; }
}

public sealed class RuleCandidateRow
{
    public long CandidateId { get; init; }
    public int NidMember { get; init; }
    public long SourceNidHistory { get; init; }
    public DateTime SourceModifyAt { get; init; }
    public string CanonicalXmlHash { get; init; } = "";
    public string XmlBody { get; init; } = "";
    public string? Modifyer { get; init; }
    public string? ModifyDesc { get; init; }
    public string Status { get; init; } = "Detected";
    public string? RejectReason { get; init; }
    public DateTime StableEligibleAtUtc { get; init; }
}

public static class RuleCandidateStatus
{
    public const string Detected = "Detected";
    public const string Parsing = "Parsing";
    public const string Parsed = "Parsed";
    public const string Validated = "Validated";
    public const string DryRunPassed = "DryRunPassed";
    public const string Stable = "Stable";
    public const string Rejected = "Rejected";
    public const string Superseded = "Superseded";
}

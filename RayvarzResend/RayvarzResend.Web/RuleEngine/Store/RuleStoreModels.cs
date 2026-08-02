namespace RayvarzResend.Web.RuleEngine.Store;

public sealed class RuleSyncStateRow
{
    public int NidMember { get; set; }
    public int NidClass { get; set; } = 360;
    public long? LastSeenNidHistory { get; set; }
    public DateTime? LastSeenModifyAt { get; set; }
    public long? LastStableNidHistory { get; set; }
    public DateTime? LastStableModifyAt { get; set; }
    public string? LastStableXmlHash { get; set; }
    public int ActiveDslVersion { get; set; }
    public string ActiveEngine { get; set; } = "Legacy";
    public long? ActiveSnapshotId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int ConsecutiveDynamicFailures { get; set; }
    public DateTime? CircuitBreakerOpenUntilUtc { get; set; }
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
    /// <summary>DocumentItem.Center</summary>
    public long? ExpectedCenter { get; init; }
    public long? ExpectedCenter1 { get; init; }
    public long? ExpectedCenter2 { get; init; }
    public long? ExpectedCenter3 { get; init; }
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
    public const string Promoted = "Promoted";
    public const string Rejected = "Rejected";
    public const string Superseded = "Superseded";
}

public sealed class RulePromotionLogRow
{
    public long LogId { get; init; }
    public int NidMember { get; init; }
    public long? CandidateId { get; init; }
    public long? SnapshotId { get; init; }
    public string Action { get; init; } = "";
    public string? Reason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class RuleDslSnapshotRow
{
    public long SnapshotId { get; init; }
    public int NidMember { get; init; }
    public int DslVersion { get; init; }
    public string XmlHash { get; init; } = "";
    public string? DslJson { get; init; }
    public string ParserVersion { get; init; } = "2.0.0";
    public string EntryPoint { get; init; } = "Run";
    public DateTime CreatedAtUtc { get; init; }
    public bool IsActive { get; init; }
}

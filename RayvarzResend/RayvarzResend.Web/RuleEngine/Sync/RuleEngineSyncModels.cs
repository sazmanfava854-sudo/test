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

    /// <summary>Open | Blocked (database/12_RuleEngine_SendGate.sql). NULL در DB = Open.</summary>
    public string SendGate { get; set; } = RuleSendGateStates.Open;

    /// <summary>هش XmlBody که مدیر پس از به‌روزرسانی کد برای ارسال تأیید کرده است.</summary>
    public string ApprovedSendXmlHash { get; set; } = "";

    public DateTime? SendGateChangedAtUtc { get; set; }
    public string? SendGateChangedBy { get; set; }
}

public static class RuleSendGateStates
{
    public const string Open = "Open";
    public const string Blocked = "Blocked";

    public static bool IsBlocked(string? gate) =>
        string.Equals(gate?.Trim(), Blocked, StringComparison.OrdinalIgnoreCase);
}

public sealed record RuleSendGateDecision(string Gate, string ApprovedHash, string? Action, string? Reason)
{
    public bool Changed => Action != null;
}

/// <summary>
/// ارسال از لحظه Detected بسته است: هر XmlBody فعال که با ApprovedSendXmlHash یکی نباشد → Blocked.
/// فقط تأیید مدیر (یا بازگشت Member به همان نسخه تأییدشده) دوباره Open می‌کند؛ ثبات ۷۲ساعته روی Gate اثری ندارد.
/// </summary>
public static class RuleSendGatePolicy
{
    public static RuleSendGateDecision Evaluate(
        string? currentGate,
        string? approvedHash,
        string? lastStableHash,
        string memberHash,
        bool isBaseline)
    {
        var blocked = RuleSendGateStates.IsBlocked(currentGate);
        var approved = approvedHash?.Trim() ?? "";

        if (approved.Length == 0)
        {
            if (isBaseline || string.IsNullOrWhiteSpace(lastStableHash))
                return new RuleSendGateDecision(RuleSendGateStates.Open, memberHash, RulePromotionActions.SendApproved,
                    "initial approved hash = current Member (baseline)");
            approved = lastStableHash!.Trim();
        }

        var matches = RuleHistoryChecker.HashEquals(approved, memberHash);
        if (!matches)
            return blocked
                ? new RuleSendGateDecision(RuleSendGateStates.Blocked, approved, null, null)
                : new RuleSendGateDecision(RuleSendGateStates.Blocked, approved, RulePromotionActions.SendBlocked,
                    "Member XmlBody differs from approved send hash");

        return blocked
            ? new RuleSendGateDecision(RuleSendGateStates.Open, approved, RulePromotionActions.SendReopened,
                "Member reverted to approved XmlBody")
            : new RuleSendGateDecision(RuleSendGateStates.Open, approved,
                string.Equals(approved, approvedHash?.Trim(), StringComparison.Ordinal) ? null : RulePromotionActions.SendApproved,
                "approved send hash initialised from LastStableXmlHash");
    }
}

public sealed class RuleSendGateRequest
{
    public string? Note { get; set; }
}

public sealed class RuleSendGateChangeResult
{
    public bool Ok { get; set; } = true;
    public string SendGate { get; set; } = RuleSendGateStates.Open;
    public string? ApprovedSendXmlHash { get; set; }
    public string? MemberHash { get; set; }
    public long? OpenCandidateId { get; set; }
    public string? Error { get; set; }
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
    public const string SendBlocked = "SendBlocked";
    public const string SendApproved = "SendApproved";
    public const string SendReopened = "SendReopened";
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
    public string SendGate { get; set; } = RuleSendGateStates.Open;
    public string? Error { get; set; }
}

namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>مقایسه Member فعال و MemberHistory با RuleSyncState (LastStableXmlHash / LastSeenNidHistory).</summary>
public sealed class RuleHistoryChecker
{
    public RuleChangeAssessment Assess(
        RuleSyncStateRow state,
        string currentMemberHash,
        MemberHistoryHead? latestHistory)
    {
        var activeHash = state.LastStableXmlHash ?? "";
        var assessment = new RuleChangeAssessment
        {
            CurrentMemberHash = currentMemberHash,
            StoredActiveHash = activeHash,
            MemberHashChanged = !string.IsNullOrEmpty(currentMemberHash)
                && !string.Equals(currentMemberHash, activeHash, StringComparison.OrdinalIgnoreCase),
        };

        if (latestHistory != null)
        {
            assessment.LatestNidHistory = latestHistory.NidHistory;
            assessment.LatestHistoryModifyUtc = latestHistory.ModifyUtc;
            assessment.HistoryAdvanced = state.LastSeenNidHistory == null
                || latestHistory.NidHistory > state.LastSeenNidHistory.Value;

            var historyHash = XmlBodyCanonicalHash.Compute(latestHistory.XmlBody);
            assessment.LatestHistoryHash = historyHash;
            assessment.HistoryHashDiffersFromMember = !string.IsNullOrEmpty(historyHash)
                && !string.Equals(historyHash, currentMemberHash, StringComparison.OrdinalIgnoreCase);
        }

        assessment.ShouldOpenCandidate = assessment.MemberHashChanged || assessment.HistoryAdvanced;

        return assessment;
    }
}

public sealed class RuleChangeAssessment
{
    public string CurrentMemberHash { get; set; } = "";
    public string StoredActiveHash { get; set; } = "";
    public bool MemberHashChanged { get; set; }
    public long? LatestNidHistory { get; set; }
    public DateTime? LatestHistoryModifyUtc { get; set; }
    public string? LatestHistoryHash { get; set; }
    public bool HistoryAdvanced { get; set; }
    public bool HistoryHashDiffersFromMember { get; set; }
    public bool ShouldOpenCandidate { get; set; }
}

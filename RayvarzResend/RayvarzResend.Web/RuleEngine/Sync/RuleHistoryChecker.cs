namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>مقایسه Member فعال و MemberHistory با state ذخیره‌شده.</summary>
public sealed class RuleHistoryChecker
{
    public RuleChangeAssessment Assess(
        RuleSyncStateRow state,
        string currentMemberHash,
        MemberHistoryHead? latestHistory)
    {
        var assessment = new RuleChangeAssessment
        {
            CurrentMemberHash = currentMemberHash,
            StoredActiveHash = state.ActiveXmlBodyHash,
            StoredLastMemberHash = state.LastMemberXmlBodyHash,
            MemberHashChanged = !string.IsNullOrEmpty(currentMemberHash)
                && !string.Equals(currentMemberHash, state.ActiveXmlBodyHash, StringComparison.OrdinalIgnoreCase),
            MemberHashDrift = !string.IsNullOrEmpty(currentMemberHash)
                && !string.Equals(currentMemberHash, state.LastMemberXmlBodyHash, StringComparison.OrdinalIgnoreCase)
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

        assessment.ShouldOpenCandidate = assessment.MemberHashChanged
            || assessment.HistoryAdvanced
            || assessment.MemberHashDrift;

        return assessment;
    }
}

public sealed class RuleChangeAssessment
{
    public string CurrentMemberHash { get; set; } = "";
    public string StoredActiveHash { get; set; } = "";
    public string StoredLastMemberHash { get; set; } = "";
    public bool MemberHashChanged { get; set; }
    public bool MemberHashDrift { get; set; }
    public long? LatestNidHistory { get; set; }
    public DateTime? LatestHistoryModifyUtc { get; set; }
    public string? LatestHistoryHash { get; set; }
    public bool HistoryAdvanced { get; set; }
    public bool HistoryHashDiffersFromMember { get; set; }
    public bool ShouldOpenCandidate { get; set; }
}

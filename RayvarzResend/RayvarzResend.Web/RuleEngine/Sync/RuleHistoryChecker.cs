namespace RayvarzResend.Web.RuleEngine.Sync;

public enum RuleSyncDecision
{
    /// <summary>اولین اجرا — نسخه فعلی Member به‌عنوان پایه ثبت می‌شود.</summary>
    Baseline,
    /// <summary>هش Member با LastStableXmlHash برابر است.</summary>
    Unchanged,
    /// <summary>candidate باز همین هش را دنبال می‌کند.</summary>
    TrackingOpenCandidate,
    /// <summary>محتوای جدید — candidate جدید (یا reopen) با شروع مجدد ساعت ثبات.</summary>
    NewCandidate
}

/// <summary>مقایسه هش XmlBody فعال Member با RuleSyncState.LastStableXmlHash و candidate باز.</summary>
public sealed class RuleHistoryChecker
{
    public RuleSyncDecision Decide(RuleSyncStateRow state, string memberHash, RuleCandidateRow? openCandidate)
    {
        if (string.IsNullOrWhiteSpace(state.LastStableXmlHash))
            return RuleSyncDecision.Baseline;

        if (HashEquals(memberHash, state.LastStableXmlHash))
            return RuleSyncDecision.Unchanged;

        if (openCandidate != null && HashEquals(memberHash, openCandidate.CanonicalXmlHash))
            return RuleSyncDecision.TrackingOpenCandidate;

        return RuleSyncDecision.NewCandidate;
    }

    public static bool HashEquals(string? a, string? b) =>
        string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
}

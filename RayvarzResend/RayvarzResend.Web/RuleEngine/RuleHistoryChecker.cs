using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine;

public sealed class RuleHistoryCheckResult
{
    public bool HasNewChange { get; init; }
    public MemberHistoryRecord? Latest { get; init; }
    public string? CanonicalXmlHash { get; init; }
}

public sealed class RuleHistoryChecker
{
    private readonly MemberRuleRepository _memberRepo;

    public RuleHistoryChecker(MemberRuleRepository memberRepo) => _memberRepo = memberRepo;

    public async Task<RuleHistoryCheckResult> CheckAsync(RuleSyncStateRow? state, int nidMember, CancellationToken ct = default)
    {
        var latest = await _memberRepo.LoadLatestHistoryAsync(nidMember, ct);
        if (latest == null || string.IsNullOrWhiteSpace(latest.XmlBody))
            return new RuleHistoryCheckResult { HasNewChange = false, Latest = latest };

        if (state?.LastSeenNidHistory is long seen && seen >= latest.NidHistory)
            return new RuleHistoryCheckResult { HasNewChange = false, Latest = latest };

        string hash;
        try
        {
            var canonical = XmlCanonicalizer.Normalize(latest.XmlBody);
            hash = RuleHashService.ComputeSha256Hex(canonical);
        }
        catch
        {
            return new RuleHistoryCheckResult { HasNewChange = true, Latest = latest };
        }

        if (!string.IsNullOrEmpty(state?.LastStableXmlHash) &&
            hash.Equals(state.LastStableXmlHash, StringComparison.OrdinalIgnoreCase))
        {
            return new RuleHistoryCheckResult { HasNewChange = false, Latest = latest, CanonicalXmlHash = hash };
        }

        return new RuleHistoryCheckResult { HasNewChange = true, Latest = latest, CanonicalXmlHash = hash };
    }
}

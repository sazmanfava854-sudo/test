using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine;

/// <summary>فاز ۰: sync تغییرات MemberHistory → RuleCandidate. Parse/Promote در فاز ۲+.</summary>
public sealed class RuleVersionManager
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly RuleHistoryChecker _historyChecker;
    private readonly ILogger<RuleVersionManager> _logger;

    public RuleVersionManager(
        IConfiguration config,
        RuleEngineStore store,
        RuleHistoryChecker historyChecker,
        ILogger<RuleVersionManager> logger)
    {
        _config = config;
        _store = store;
        _historyChecker = historyChecker;
        _logger = logger;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    public int StabilityHours => _config.GetValue("RuleEngine:StabilityHours", 72);

    public async Task<RuleSyncStateRow> InitializeAsync(CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct)
            ?? new RuleSyncStateRow { NidMember = NidMember, NidClass = 360, ActiveEngine = "Legacy", ActiveDslVersion = 0 };

        await EvaluateChangesAsync(state, ct);
        return await _store.GetSyncStateAsync(NidMember, ct) ?? state;
    }

    public async Task EvaluateChangesAsync(RuleSyncStateRow? state = null, CancellationToken ct = default)
    {
        if (!_store.IsConfigured)
        {
            _logger.LogWarning("RayvarzRuleEngine connection not configured — skip rule sync");
            return;
        }

        state ??= await _store.GetSyncStateAsync(NidMember, ct)
            ?? new RuleSyncStateRow { NidMember = NidMember, NidClass = 360, ActiveEngine = "Legacy", ActiveDslVersion = 0 };

        var check = await _historyChecker.CheckAsync(state, NidMember, ct);
        if (check.Latest == null)
        {
            _logger.LogInformation("No MemberHistory for NidMember {NidMember}", NidMember);
            return;
        }

        state.LastSeenNidHistory = check.Latest.NidHistory;
        state.LastSeenModifyAt = check.Latest.ModifyDateTime;

        if (!check.HasNewChange)
        {
            await _store.UpsertSyncStateAsync(state, ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(check.CanonicalXmlHash))
        {
            try
            {
                var canonical = XmlCanonicalizer.Normalize(check.Latest.XmlBody);
                check = check with { CanonicalXmlHash = RuleHashService.ComputeSha256Hex(canonical) };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid XmlBody in MemberHistory NidHistory={NidHistory}", check.Latest.NidHistory);
                await _store.InsertPromotionLogAsync(NidMember, null, null, "Rejected", "Invalid XML in MemberHistory", ct);
                await _store.UpsertSyncStateAsync(state, ct);
                return;
            }
        }

        var hash = check.CanonicalXmlHash!;
        if (!await _store.CandidateExistsAsync(NidMember, hash, ct))
        {
            string canonicalXml;
            try { canonicalXml = XmlCanonicalizer.Normalize(check.Latest.XmlBody); }
            catch { canonicalXml = check.Latest.XmlBody; }

            var stableAt = check.Latest.ModifyDateTime.ToUniversalTime().AddHours(StabilityHours);
            await _store.InsertCandidateAsync(new RuleCandidateRow
            {
                NidMember = NidMember,
                SourceNidHistory = check.Latest.NidHistory,
                SourceModifyAt = check.Latest.ModifyDateTime,
                CanonicalXmlHash = hash,
                XmlBody = canonicalXml,
                Modifyer = check.Latest.Modifyer,
                ModifyDesc = check.Latest.ModifyDesc,
                Status = RuleCandidateStatus.Detected,
                StableEligibleAtUtc = stableAt
            }, ct);

            _logger.LogInformation(
                "New rule candidate NidHistory={NidHistory} hash={HashPrefix}… stable after {StableAt:u}",
                check.Latest.NidHistory, hash[..12], stableAt);
        }

        await _store.UpsertSyncStateAsync(state, ct);
    }
}

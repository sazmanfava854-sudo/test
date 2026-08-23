using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine;

/// <summary>فاز ۰–۲: sync MemberHistory → RuleCandidate؛ parse XmlBody → RuleDslSnapshot (بدون Active).</summary>
public sealed class RuleVersionManager
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly RuleHistoryChecker _historyChecker;
    private readonly RuleDslParserService _dslParser;
    private readonly ILogger<RuleVersionManager> _logger;

    public RuleVersionManager(
        IConfiguration config,
        RuleEngineStore store,
        RuleHistoryChecker historyChecker,
        RuleDslParserService dslParser,
        ILogger<RuleVersionManager> logger)
    {
        _config = config;
        _store = store;
        _historyChecker = historyChecker;
        _dslParser = dslParser;
        _logger = logger;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    public int StabilityHours => _config.GetValue("RuleEngine:StabilityHours", 72);

    public async Task<RuleSyncStateRow> InitializeAsync(CancellationToken ct = default)
    {
        if (!await EnsureSchemaReadyAsync(ct))
            return new RuleSyncStateRow { NidMember = NidMember, NidClass = 360, ActiveEngine = "Legacy", ActiveDslVersion = 0 };

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

        if (!await EnsureSchemaReadyAsync(ct))
            return;

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

        var hash = check.CanonicalXmlHash;
        if (string.IsNullOrWhiteSpace(hash))
        {
            try
            {
                var canonical = XmlCanonicalizer.Normalize(check.Latest.XmlBody);
                hash = RuleHashService.ComputeSha256Hex(canonical);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid XmlBody in MemberHistory NidHistory={NidHistory}", check.Latest.NidHistory);
                await _store.InsertPromotionLogAsync(NidMember, null, null, "Rejected", "Invalid XML in MemberHistory", ct);
                await _store.UpsertSyncStateAsync(state, ct);
                return;
            }
        }

        if (!await _store.CandidateExistsAsync(NidMember, hash, ct))
        {
            string canonicalXml;
            try { canonicalXml = XmlCanonicalizer.Normalize(check.Latest.XmlBody); }
            catch { canonicalXml = check.Latest.XmlBody; }

            var stableAt = check.Latest.ModifyDateTime.ToUniversalTime().AddHours(StabilityHours);
            var candidateId = await _store.InsertCandidateAsync(new RuleCandidateRow
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

            await TryParseCandidateAsync(candidateId, canonicalXml, ct);
        }

        await _store.UpsertSyncStateAsync(state, ct);
    }

    public async Task<DslPersistResult> ParseActiveMemberSnapshotAsync(CancellationToken ct = default) =>
        await _dslParser.ParseActiveMemberAsync(ct);

    private async Task TryParseCandidateAsync(long candidateId, string xmlBody, CancellationToken ct)
    {
        try
        {
            await _store.UpdateCandidateStatusAsync(candidateId, RuleCandidateStatus.Parsing, ct: ct);
            var result = await _dslParser.ParseAndStoreAsync(xmlBody, "MemberHistory", ct);
            if (result.Parse?.Success == true)
            {
                await _store.UpdateCandidateStatusAsync(candidateId, RuleCandidateStatus.Parsed, ct: ct);
                await _store.InsertPromotionLogAsync(
                    NidMember, candidateId, result.SnapshotId, "Parsed",
                    result.SkippedExisting ? "DSL snapshot already existed" : "DSL snapshot stored", ct);
            }
            else
            {
                await _store.UpdateCandidateStatusAsync(
                    candidateId, RuleCandidateStatus.Rejected,
                    result.Parse?.ErrorMessage ?? result.Message, ct);
                await _store.InsertPromotionLogAsync(
                    NidMember, candidateId, null, "Rejected",
                    result.Parse?.ErrorMessage ?? result.Message, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DSL parse failed for candidate {CandidateId}", candidateId);
            await _store.UpdateCandidateStatusAsync(candidateId, RuleCandidateStatus.Rejected, ex.Message, ct);
        }
    }

    private async Task<bool> EnsureSchemaReadyAsync(CancellationToken ct)
    {
        var diag = await _store.GetDiagnosticsAsync(ct);
        if (diag.SchemaReady)
            return true;

        _logger.LogWarning(
            "RayvarzRuleEngine schema not ready. Configured DB={ConfiguredDb}, Actual DB={ActualDb}, Tables=[{Tables}]. {Message}",
            diag.ConfiguredDatabase,
            diag.ActualDatabase,
            string.Join(", ", diag.ExistingTables),
            diag.Message);
        return false;
    }
}

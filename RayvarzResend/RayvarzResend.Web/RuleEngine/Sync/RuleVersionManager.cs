using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleVersionManager
{
    private readonly RuleEngineStateRepository _state;
    private readonly MemberRuleRepository _memberRules;
    private readonly RuleHistoryChecker _historyChecker;
    private readonly RuleGoldenDryRunService _dryRun;
    private readonly RuleEngineSyncOptions _options;
    private readonly ILogger<RuleVersionManager> _logger;

    public RuleVersionManager(
        RuleEngineStateRepository state,
        MemberRuleRepository memberRules,
        RuleHistoryChecker historyChecker,
        RuleGoldenDryRunService dryRun,
        IOptions<RuleEngineSyncOptions> options,
        ILogger<RuleVersionManager> logger)
    {
        _state = state;
        _memberRules = memberRules;
        _historyChecker = historyChecker;
        _dryRun = dryRun;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RuleSyncCycleResult> RunCycleAsync(CancellationToken ct = default)
    {
        var result = new RuleSyncCycleResult();
        try
        {
            if (!_state.IsConfigured)
            {
                result.Ok = false;
                result.Error = "Rule sync state DB not configured (AppAuth / RayvarzRuleEngine).";
                return result;
            }

            await _state.EnsureSchemaAsync(ct);

            var nidMember = _options.NidMember;
            var state = await _state.GetOrCreateStateAsync(nidMember, ct);

            var member = await _memberRules.LoadActiveMemberAsync(nidMember, ct: ct);
            if (member is null)
            {
                result.Ok = false;
                result.Error = $"Member {nidMember} not found (RuleEngine connection or LocalXmlPath).";
                return result;
            }

            var history = await _memberRules.LoadLatestHistoryAsync(nidMember, ct);
            var memberHash = XmlBodyCanonicalHash.Compute(member.XmlBody);
            var assessment = _historyChecker.Assess(state, memberHash, history);

            result.MemberHash = memberHash;
            result.ActiveHash = state.LastStableXmlHash;
            result.MemberHashChanged = assessment.MemberHashChanged;
            result.LatestNidHistory = assessment.LatestNidHistory;
            result.HistoryAdvanced = assessment.HistoryAdvanced;

            if (history != null)
            {
                state.LastSeenNidHistory = history.NidHistory;
                state.LastSeenModifyAt = history.ModifyUtc;
            }

            await _state.UpdateStateAsync(state, ct);

            if (!assessment.ShouldOpenCandidate)
            {
                result.Summary = "no change";
                result.Promoted = await TryPromoteStableCandidateAsync(nidMember, state, ct);
                return result;
            }

            if (history is null || string.IsNullOrWhiteSpace(history.XmlBody))
            {
                result.Summary = "change detected but MemberHistory XmlBody missing";
                return result;
            }

            var hash = XmlBodyCanonicalHash.Compute(history.XmlBody);
            var open = await _state.GetOpenCandidateAsync(nidMember, ct);
            if (open is not null && string.Equals(open.CanonicalXmlHash, hash, StringComparison.OrdinalIgnoreCase))
            {
                await _state.TouchCandidateCheckedAsync(open.CandidateId, ct);
                result.Summary = "open candidate already tracks latest history hash";
                result.CandidateId = open.CandidateId;
                result.Promoted = await TryPromoteStableCandidateAsync(nidMember, state, ct);
                return result;
            }

            var stableAt = DateTime.UtcNow.AddHours(_options.StabilityHours);
            var candidateId = await _state.InsertCandidateAsync(
                nidMember,
                history.NidHistory,
                history.ModifyUtc,
                hash,
                history.XmlBody,
                history.Modifyer,
                history.ModifyDesc,
                stableAt,
                ct);

            result.CandidateId = candidateId;
            result.Summary = $"candidate {candidateId} opened; stable after {stableAt:O}";

            _logger.LogInformation(
                "Rule sync: candidate {CandidateId} for NidMember={NidMember}, stableEligibleAt={StableAt}",
                candidateId, nidMember, stableAt);

            result.Promoted = await TryPromoteStableCandidateAsync(nidMember, state, ct);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rule sync cycle failed");
            result.Ok = false;
            result.Error = ex.Message;
            return result;
        }
    }

    private async Task<bool> TryPromoteStableCandidateAsync(int nidMember, RuleSyncStateRow state, CancellationToken ct)
    {
        var candidate = await _state.GetStableEligibleCandidateAsync(nidMember, _options.StabilityHours, ct);
        if (candidate is null)
            return false;

        var dryRun = await _dryRun.RunForCandidateAsync(candidate, ct);
        if (!dryRun.Success)
        {
            _logger.LogWarning(
                "Rule sync: dry-run failed for candidate {CandidateId}: {Error}",
                candidate.CandidateId, dryRun.ErrorMessage);
            await _state.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatuses.Rejected, dryRun.ErrorMessage, ct);
            await _state.InsertPromotionLogAsync(
                nidMember,
                candidate.CandidateId,
                dryRun.SnapshotId,
                RulePromotionActions.Rejected,
                dryRun.ErrorMessage ?? "dry-run failed",
                ct);
            return false;
        }

        if (!_options.AutoPromoteDynamic)
        {
            _logger.LogInformation(
                "Rule sync: candidate {CandidateId} passed dry-run; AutoPromoteDynamic=false",
                candidate.CandidateId);
            return false;
        }

        await PromoteCandidateCoreAsync(nidMember, state, candidate, dryRun.SnapshotId, ct);
        return true;
    }

    public async Task PromoteCandidateAsync(int nidMember, long candidateId, CancellationToken ct = default)
    {
        var state = await _state.GetOrCreateStateAsync(nidMember, ct);
        var candidate = await _state.GetCandidateByIdAsync(candidateId, ct)
            ?? throw new InvalidOperationException($"Candidate {candidateId} not found");

        if (string.Equals(candidate.Status, RuleCandidateStatuses.Promoted, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Status, RuleCandidateStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Candidate {candidateId} status is {candidate.Status}");

        var dryRun = await _dryRun.RunForCandidateAsync(candidate, ct);
        if (!dryRun.Success)
            throw new InvalidOperationException(dryRun.ErrorMessage ?? "dry-run failed");

        await PromoteCandidateCoreAsync(nidMember, state, candidate, dryRun.SnapshotId, ct);
    }

    private async Task PromoteCandidateCoreAsync(
        int nidMember,
        RuleSyncStateRow state,
        RuleCandidateRow candidate,
        long? snapshotId,
        CancellationToken ct)
    {
        var fromEngine = state.ActiveEngine;
        var fromHash = state.LastStableXmlHash;

        state.LastStableNidHistory = candidate.SourceNidHistory;
        state.LastStableModifyAt = candidate.SourceModifyAt;
        state.LastStableXmlHash = candidate.CanonicalXmlHash;
        state.ActiveEngine = "Dynamic";
        state.ActiveDslVersion = null;
        state.ActiveSnapshotId = snapshotId;
        state.ConsecutiveDynamicFailures = 0;
        await _state.UpdateStateAsync(state, ct);

        await _state.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatuses.Promoted, null, ct);
        await _state.InsertPromotionLogAsync(
            nidMember,
            candidate.CandidateId,
            snapshotId,
            RulePromotionActions.Promoted,
            $"from={fromEngine} hash={fromHash}",
            ct);

        _logger.LogInformation(
            "Rule sync: promoted candidate {CandidateId} for NidMember={NidMember}",
            candidate.CandidateId, nidMember);
    }
}

using Microsoft.Extensions.Options;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleVersionManager
{
    private readonly RuleEngineSyncOptions _options;
    private readonly RuleEngineStateRepository _state;
    private readonly MemberRuleRepository _members;
    private readonly RuleHistoryChecker _checker;
    private readonly RuleGoldenDryRunService _dryRun;
    private readonly ILogger<RuleVersionManager> _logger;

    public RuleVersionManager(
        IOptions<RuleEngineSyncOptions> options,
        RuleEngineStateRepository state,
        MemberRuleRepository members,
        RuleHistoryChecker checker,
        RuleGoldenDryRunService dryRun,
        ILogger<RuleVersionManager> logger)
    {
        _options = options.Value;
        _state = state;
        _members = members;
        _checker = checker;
        _dryRun = dryRun;
        _logger = logger;
    }

    public async Task<RuleSyncCycleResult> RunCycleAsync(CancellationToken ct = default)
    {
        if (!_options.Enabled)
            return new RuleSyncCycleResult { Ok = true, Summary = "RuleEngineSync غیرفعال است (RuleEngineSync:Enabled=false)." };

        if (!_state.IsConfigured)
            return new RuleSyncCycleResult { Ok = false, Error = "پایگاه RuleSyncState پیکربندی نشده (ConnectionStrings:AppAuth)." };

        var nid = _options.NidMember;
        var result = new RuleSyncCycleResult();
        try
        {
            var syncState = await _state.GetOrCreateStateAsync(nid, ct);
            var member = await _members.LoadActiveMemberAsync(nid, ct: ct);
            if (member == null || string.IsNullOrWhiteSpace(member.XmlBody))
            {
                syncState.LastCheckAtUtc = DateTime.UtcNow;
                syncState.LastError = "Member یا XmlBody از DbRuleEngein خوانده نشد.";
                await _state.UpdateStateAsync(syncState, ct);
                return new RuleSyncCycleResult { Ok = false, Error = syncState.LastError };
            }

            var memberHash = XmlBodyCanonicalHash.Compute(member.XmlBody);
            if (string.IsNullOrEmpty(syncState.ActiveXmlBodyHash) && !string.IsNullOrEmpty(memberHash))
            {
                syncState.ActiveXmlBodyHash = memberHash;
                syncState.ActiveMemberVersion = member.Version;
            }

            var history = await _members.LoadLatestHistoryAsync(nid, ct);
            var assessment = _checker.Assess(syncState, memberHash, history);

            result.MemberHash = memberHash;
            result.ActiveHash = syncState.ActiveXmlBodyHash;
            result.MemberHashChanged = assessment.MemberHashChanged;
            result.LatestNidHistory = assessment.LatestNidHistory;
            result.HistoryAdvanced = assessment.HistoryAdvanced;

            syncState.LastCheckAtUtc = DateTime.UtcNow;
            syncState.LastMemberXmlBodyHash = memberHash;
            syncState.LastError = null;
            if (history != null)
            {
                syncState.LastSeenNidHistory = history.NidHistory;
                syncState.LastHistoryModifyUtc = history.ModifyUtc;
            }

            var candidate = await _state.GetOpenCandidateAsync(nid, ct);
            if (assessment.ShouldOpenCandidate && (candidate == null || candidate.XmlBodyHash != memberHash))
            {
                if (candidate != null)
                {
                    candidate.State = RuleCandidateState.Rejected;
                    candidate.Notes = "نسخه جدید Member/History شناسایی شد.";
                    await _state.UpdateCandidateAsync(candidate, ct);
                }

                candidate = new RuleCandidateRow
                {
                    Id = Guid.NewGuid(),
                    NidMember = nid,
                    NidHistory = history?.NidHistory,
                    XmlBodyHash = memberHash,
                    MemberVersion = member.Version,
                    State = RuleCandidateState.Detected,
                    DetectedAtUtc = DateTime.UtcNow,
                    Notes = BuildDetectionNotes(assessment)
                };
                await _state.InsertCandidateAsync(candidate, ct);
                syncState.LastChangeDetectedAtUtc = DateTime.UtcNow;
                result.CandidateId = candidate.Id;
                _logger.LogInformation("Rule sync: candidate {CandidateId} hash={Hash}", candidate.Id, memberHash);
            }

            candidate = await _state.GetOpenCandidateAsync(nid, ct);
            if (candidate != null)
                await AdvanceCandidateAsync(candidate, member, memberHash, history, syncState, ct);

            await _state.UpdateStateAsync(syncState, ct);

            result.Ok = true;
            result.Summary = candidate == null
                ? "بدون candidate باز؛ هش Member با Active مقایسه شد."
                : $"candidate {candidate.State} hash={candidate.XmlBodyHash}";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rule sync cycle failed");
            return new RuleSyncCycleResult { Ok = false, Error = ex.Message };
        }
    }

    private async Task AdvanceCandidateAsync(
        RuleCandidateRow candidate,
        MemberRuleRecord member,
        string memberHash,
        MemberHistoryHead? history,
        RuleSyncStateRow syncState,
        CancellationToken ct)
    {
        if (candidate.State == RuleCandidateState.Detected)
        {
            try
            {
                ClsFunctionParser.Parse(member.XmlBody);
                candidate.ParseOk = true;
                candidate.State = RuleCandidateState.Parsed;
                candidate.ValidationOk = true;
                candidate.State = RuleCandidateState.Validated;
            }
            catch (Exception ex)
            {
                candidate.ParseOk = false;
                candidate.State = RuleCandidateState.Rejected;
                candidate.Notes = $"Parse failed: {ex.Message}";
                await _state.UpdateCandidateAsync(candidate, ct);
                await _state.InsertPromotionLogAsync(candidate.NidMember, candidate.Id, syncState.ActiveEngine,
                    syncState.ActiveEngine, memberHash, history?.NidHistory, "Rejected", candidate.Notes, ct);
                return;
            }
        }

        if (!string.Equals(candidate.XmlBodyHash, memberHash, StringComparison.OrdinalIgnoreCase))
        {
            candidate.State = RuleCandidateState.Rejected;
            candidate.Notes = "هش candidate با Member فعال یکی نیست.";
            await _state.UpdateCandidateAsync(candidate, ct);
            return;
        }

        if (history != null && candidate.NidHistory.HasValue && history.NidHistory > candidate.NidHistory.Value)
        {
            candidate.StableSinceUtc = null;
            candidate.NidHistory = history.NidHistory;
            candidate.Notes = "MemberHistory جدید — دوره ثبات ۷۲ساعته از نو شروع می‌شود.";
        }
        else if (!candidate.StableSinceUtc.HasValue)
        {
            candidate.StableSinceUtc = DateTime.UtcNow;
        }

        var stability = TimeSpan.FromHours(Math.Max(1, _options.StabilityHours));
        var stableEnough = candidate.StableSinceUtc.HasValue
            && DateTime.UtcNow - candidate.StableSinceUtc.Value >= stability;

        if (candidate.State is RuleCandidateState.Validated or RuleCandidateState.Stable)
        {
            if (!stableEnough)
            {
                candidate.State = RuleCandidateState.Stable;
                await _state.UpdateCandidateAsync(candidate, ct);
                return;
            }

            var (dryOk, drySummary) = await _dryRun.RunStructuralPlaceholderAsync(candidate.Id, candidate.NidMember, ct);
            candidate.DryRunOk = dryOk;
            candidate.Notes = drySummary;
            if (!dryOk)
            {
                candidate.State = RuleCandidateState.Stable;
                await _state.UpdateCandidateAsync(candidate, ct);
                return;
            }

            candidate.State = RuleCandidateState.PromoteReady;
            await _state.UpdateCandidateAsync(candidate, ct);
            await TryPromoteAsync(candidate, syncState, ct);
        }
    }

    private async Task TryPromoteAsync(RuleCandidateRow candidate, RuleSyncStateRow syncState, CancellationToken ct)
    {
        var from = syncState.ActiveEngine;
        var to = _options.AutoPromoteDynamic ? "Dynamic" : "Legacy";
        var outcome = _options.AutoPromoteDynamic ? "Promoted" : "ReadyNotAuto";

        syncState.ActiveXmlBodyHash = candidate.XmlBodyHash;
        syncState.ActiveMemberVersion = candidate.MemberVersion;
        if (_options.AutoPromoteDynamic)
        {
            syncState.ActiveEngine = "Dynamic";
            syncState.LastPromotedAtUtc = DateTime.UtcNow;
        }

        candidate.State = _options.AutoPromoteDynamic ? RuleCandidateState.Promoted : RuleCandidateState.PromoteReady;
        await _state.UpdateCandidateAsync(candidate, ct);
        await _state.UpdateStateAsync(syncState, ct);
        await _state.InsertPromotionLogAsync(
            candidate.NidMember,
            candidate.Id,
            from,
            to,
            candidate.XmlBodyHash,
            candidate.NidHistory,
            outcome,
            _options.AutoPromoteDynamic
                ? "AutoPromoteDynamic=true — ActiveEngine=Dynamic"
                : "شرط‌ها برقرار؛ برای فعال‌سازی Dynamic، AutoPromoteDynamic=true یا promote دستی.",
            ct);

        _logger.LogInformation("Rule promote {Outcome} member={Nid} hash={Hash}", outcome, candidate.NidMember, candidate.XmlBodyHash);
    }

    private static string BuildDetectionNotes(RuleChangeAssessment a)
    {
        var parts = new List<string>();
        if (a.MemberHashChanged) parts.Add("MemberHash≠ActiveHash");
        if (a.HistoryAdvanced) parts.Add($"History advanced → {a.LatestNidHistory}");
        if (a.MemberHashDrift) parts.Add("MemberHash drift");
        if (a.HistoryHashDiffersFromMember) parts.Add("HistoryHash≠MemberHash");
        return string.Join("; ", parts);
    }
}

using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>
/// چرخه sync: هش XmlBody فعال Member (DbRuleEngein) با LastStableXmlHash مقایسه می‌شود؛
/// محتوای جدید → RuleCandidate با StableEligibleAtUtc = now + StabilityHours؛
/// پس از ثبات و parse موفق → LastStable* به‌روز (و در صورت AutoPromoteDynamic، ActiveEngine=Dynamic).
/// </summary>
public sealed class RuleVersionManager
{
    private readonly RuleEngineStateRepository _state;
    private readonly MemberRuleRepository _memberRules;
    private readonly RuleHistoryChecker _checker;
    private readonly RuleCandidateValidator _validator;
    private readonly RuleEngineSyncOptions _options;
    private readonly ILogger<RuleVersionManager> _logger;
    private readonly SemaphoreSlim _cycleLock = new(1, 1);

    public RuleVersionManager(
        RuleEngineStateRepository state,
        MemberRuleRepository memberRules,
        RuleHistoryChecker checker,
        RuleCandidateValidator validator,
        IOptions<RuleEngineSyncOptions> options,
        ILogger<RuleVersionManager> logger)
    {
        _state = state;
        _memberRules = memberRules;
        _checker = checker;
        _validator = validator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RuleSyncCycleResult> RunCycleAsync(CancellationToken ct = default)
    {
        await _cycleLock.WaitAsync(ct);
        try
        {
            return await RunCycleCoreAsync(ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Rule sync cycle failed");
            return new RuleSyncCycleResult { Ok = false, Error = ex.Message };
        }
        finally
        {
            _cycleLock.Release();
        }
    }

    private async Task<RuleSyncCycleResult> RunCycleCoreAsync(CancellationToken ct)
    {
        var result = new RuleSyncCycleResult();
        if (!_state.IsConfigured)
        {
            result.Ok = false;
            result.Error = $"Connection string '{_options.StateConnectionStringName}' (RayvarzRuleEngine) not configured.";
            return result;
        }

        var nidMember = _options.NidMember;
        var state = await _state.GetOrCreateStateAsync(nidMember, ct);

        var member = await _memberRules.LoadActiveMemberAsync(nidMember, ct: ct);
        if (member is null || string.IsNullOrWhiteSpace(member.XmlBody))
        {
            result.Ok = false;
            result.Error = $"Active Member {nidMember} not found or XmlBody empty (ConnectionStrings:RuleEngine).";
            return result;
        }

        var history = await _memberRules.LoadLatestHistoryAsync(nidMember, ct);
        var memberHash = XmlBodyCanonicalHash.Compute(member.XmlBody);
        var sourceNidHistory = history?.NidHistory ?? 0;
        var sourceModifyAt = history?.ModifyUtc ?? member.VersionDateTime ?? DateTime.UtcNow;

        result.MemberHash = memberHash;
        result.LatestNidHistory = history?.NidHistory;

        if (history != null)
        {
            state.LastSeenNidHistory = history.NidHistory;
            state.LastSeenModifyAt = history.ModifyUtc;
        }

        var open = await _state.GetOpenCandidateAsync(nidMember, ct);
        var decision = _checker.Decide(state, memberHash, open);

        switch (decision)
        {
            case RuleSyncDecision.Baseline:
                state.LastStableXmlHash = memberHash;
                state.LastStableNidHistory = history?.NidHistory;
                state.LastStableModifyAt = sourceModifyAt;
                await _state.UpdateStateAsync(state, ct);
                await _state.InsertPromotionLogAsync(nidMember, null, null, RulePromotionActions.Baseline,
                    $"initial baseline NidHistory={sourceNidHistory}", ct);
                result.Summary = "baseline recorded from current Member";
                break;

            case RuleSyncDecision.Unchanged:
                await _state.UpdateStateAsync(state, ct);
                await SupersedePendingAsync(nidMember, null, "Member reverted to stable XmlBody", ct);
                result.Summary = "no change";
                break;

            case RuleSyncDecision.TrackingOpenCandidate:
                await _state.UpdateStateAsync(state, ct);
                await _state.TouchCandidateCheckedAsync(open!.CandidateId, ct);
                result.CandidateId = open.CandidateId;
                result.Summary = $"candidate {open.CandidateId} waiting for stability until {open.StableEligibleAtUtc:O}";
                break;

            case RuleSyncDecision.NewCandidate:
                await _state.UpdateStateAsync(state, ct);
                var (candidateId, invalid) = await OpenCandidateAsync(nidMember, member, history, memberHash,
                    sourceNidHistory, sourceModifyAt, ct);
                await SupersedePendingAsync(nidMember, candidateId, $"superseded by candidate {candidateId}", ct);
                result.CandidateId = candidateId;
                result.Summary = invalid
                    ? $"change detected; candidate {candidateId} rejected (invalid XmlBody)"
                    : $"change detected; candidate {candidateId} opened";
                break;
        }

        await TryAcceptStableCandidateAsync(nidMember, state, memberHash, result, ct);
        result.StableHash = state.LastStableXmlHash;
        return result;
    }

    /// <summary>
    /// محتوای نامعتبر فوراً Rejected ثبت می‌شود و در چرخه‌های بعدی (تا تغییر محتوا) دوباره باز نمی‌شود.
    /// </summary>
    private async Task<(long CandidateId, bool Invalid)> OpenCandidateAsync(
        int nidMember,
        MemberRuleRecord member,
        MemberHistoryHead? history,
        string memberHash,
        long sourceNidHistory,
        DateTime sourceModifyAt,
        CancellationToken ct)
    {
        var stableAt = DateTime.UtcNow.AddHours(_options.StabilityHours);
        var row = new RuleCandidateRow
        {
            NidMember = nidMember,
            SourceNidHistory = sourceNidHistory,
            SourceModifyAt = sourceModifyAt,
            CanonicalXmlHash = memberHash,
            XmlBody = member.XmlBody,
            Modifyer = history?.Modifyer,
            ModifyDesc = history?.ModifyDesc,
            Status = RuleCandidateStatuses.Detected,
            StableEligibleAtUtc = stableAt
        };

        var validation = _validator.Validate(row);
        var existing = await _state.GetCandidateByHashAsync(nidMember, memberHash, ct);

        if (!validation.Success)
        {
            if (existing != null && string.Equals(existing.Status, RuleCandidateStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                await _state.TouchCandidateCheckedAsync(existing.CandidateId, ct);
                return (existing.CandidateId, true);
            }

            row.Status = RuleCandidateStatuses.Rejected;
            long rejectedId;
            if (existing != null)
            {
                await _state.ReopenCandidateAsync(existing.CandidateId, row, ct);
                rejectedId = existing.CandidateId;
            }
            else
            {
                rejectedId = await _state.InsertCandidateAsync(row, ct);
            }
            await RejectAsync(nidMember, new RuleCandidateRow { CandidateId = rejectedId }, validation.ErrorMessage ?? "validation failed", ct);
            return (rejectedId, true);
        }

        long candidateId;
        if (existing != null)
        {
            await _state.ReopenCandidateAsync(existing.CandidateId, row, ct);
            candidateId = existing.CandidateId;
        }
        else
        {
            candidateId = await _state.InsertCandidateAsync(row, ct);
        }

        _logger.LogInformation(
            "Rule sync: candidate {CandidateId} for NidMember={NidMember} (NidHistory={NidHistory}), stable after {StableAt:O}",
            candidateId, nidMember, sourceNidHistory, stableAt);
        return (candidateId, false);
    }

    private async Task SupersedePendingAsync(int nidMember, long? exceptId, string reason, CancellationToken ct)
    {
        var ids = await _state.SupersedePendingCandidatesAsync(nidMember, exceptId, reason, ct);
        foreach (var id in ids)
            await _state.InsertPromotionLogAsync(nidMember, id, null, RulePromotionActions.Superseded, reason, ct);
    }

    private async Task TryAcceptStableCandidateAsync(
        int nidMember,
        RuleSyncStateRow state,
        string memberHash,
        RuleSyncCycleResult result,
        CancellationToken ct)
    {
        var candidate = await _state.GetStableEligibleCandidateAsync(nidMember, ct);
        if (candidate is null)
            return;

        if (!RuleHistoryChecker.HashEquals(candidate.CanonicalXmlHash, memberHash))
        {
            await RejectAsync(nidMember, candidate, "Member XmlBody changed before stability window ended", ct);
            return;
        }

        var validation = _validator.Validate(candidate);
        if (!validation.Success)
        {
            await RejectAsync(nidMember, candidate, validation.ErrorMessage ?? "validation failed", ct);
            result.Summary += $"; candidate {candidate.CandidateId} rejected";
            return;
        }

        var breakerOpen = state.CircuitBreakerOpenUntilUtc is { } until && until > DateTime.UtcNow;
        var promote = _options.AutoPromoteDynamic && !breakerOpen;

        state.LastStableNidHistory = candidate.SourceNidHistory;
        state.LastStableModifyAt = candidate.SourceModifyAt;
        state.LastStableXmlHash = candidate.CanonicalXmlHash;
        if (promote)
        {
            state.ActiveEngine = "Dynamic";
            state.ConsecutiveDynamicFailures = 0;
        }
        await _state.UpdateStateAsync(state, ct);

        var status = promote ? RuleCandidateStatuses.Promoted : RuleCandidateStatuses.Stable;
        var action = promote ? RulePromotionActions.Promoted : RulePromotionActions.Stable;
        var reason = $"stable {_options.StabilityHours}h, functions={validation.FunctionCount}"
            + (breakerOpen ? ", circuit breaker open" : "")
            + (promote ? "" : $", engine={state.ActiveEngine}");

        await _state.UpdateCandidateStatusAsync(candidate.CandidateId, status, null, ct);
        await _state.InsertPromotionLogAsync(nidMember, candidate.CandidateId, state.ActiveSnapshotId, action, reason, ct);

        result.CandidateId = candidate.CandidateId;
        result.Accepted = true;
        result.Promoted = promote;
        result.Summary += $"; candidate {candidate.CandidateId} → {status}";

        _logger.LogInformation(
            "Rule sync: candidate {CandidateId} accepted as {Status} for NidMember={NidMember}",
            candidate.CandidateId, status, nidMember);
    }

    private async Task RejectAsync(int nidMember, RuleCandidateRow candidate, string reason, CancellationToken ct)
    {
        _logger.LogWarning("Rule sync: candidate {CandidateId} rejected: {Reason}", candidate.CandidateId, reason);
        await _state.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatuses.Rejected, reason, ct);
        await _state.InsertPromotionLogAsync(nidMember, candidate.CandidateId, null, RulePromotionActions.Rejected, reason, ct);
    }
}

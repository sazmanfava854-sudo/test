using RayvarzResend.Web.RuleEngine.Engines;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Promotion;

/// <summary>فاز ۴: Candidate → Validated → DryRunPassed → Promote (ActiveEngine=Dynamic).</summary>
public sealed class RulePromotionService
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly MemberRuleRepository _members;
    private readonly DslValidator _validator;
    private readonly GoldenDryRunService _goldenDryRun;
    private readonly DynamicRuleEngine _dynamic;
    private readonly RuleCircuitBreakerService _circuitBreaker;
    private readonly ILogger<RulePromotionService> _logger;

    public RulePromotionService(
        IConfiguration config,
        RuleEngineStore store,
        MemberRuleRepository members,
        DslValidator validator,
        GoldenDryRunService goldenDryRun,
        DynamicRuleEngine dynamic,
        RuleCircuitBreakerService circuitBreaker,
        ILogger<RulePromotionService> logger)
    {
        _config = config;
        _store = store;
        _members = members;
        _validator = validator;
        _goldenDryRun = goldenDryRun;
        _dynamic = dynamic;
        _circuitBreaker = circuitBreaker;
        _logger = logger;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    public int StabilityHours => _config.GetValue("RuleEngine:StabilityHours", 72);
    public bool EnableAutoPromote => _config.GetValue("RuleEngine:EnableAutoPromote", false);

    public async Task<RulePromotionStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct);
        var candidates = await _store.GetCandidatesByStatusesAsync(NidMember,
            new[]
            {
                RuleCandidateStatus.Detected, RuleCandidateStatus.Parsed, RuleCandidateStatus.Validated,
                RuleCandidateStatus.DryRunPassed, RuleCandidateStatus.Stable, RuleCandidateStatus.Promoted
            }, ct);
        var logs = await _store.GetRecentPromotionLogsAsync(NidMember, 15, ct);

        return new RulePromotionStatus
        {
            NidMember = NidMember,
            ActiveEngine = state?.ActiveEngine ?? "Legacy",
            ActiveDslVersion = state?.ActiveDslVersion ?? 0,
            ActiveSnapshotId = state?.ActiveSnapshotId,
            ConsecutiveDynamicFailures = state?.ConsecutiveDynamicFailures ?? 0,
            CircuitBreakerOpenUntilUtc = state?.CircuitBreakerOpenUntilUtc,
            CircuitBreakerOpen = await _circuitBreaker.IsOpenAsync(ct),
            EnableAutoPromote = EnableAutoPromote,
            StabilityHours = StabilityHours,
            Candidates = candidates.Select(c => (object)new
            {
                c.CandidateId,
                c.Status,
                c.SourceNidHistory,
                c.SourceModifyAt,
                c.StableEligibleAtUtc,
                hashPrefix = c.CanonicalXmlHash[..Math.Min(12, c.CanonicalXmlHash.Length)]
            }).ToList(),
            RecentLogs = logs
        };
    }

    public async Task<RulePromotionRunResult> EvaluatePromotionsAsync(bool forcePromote = false, CancellationToken ct = default)
    {
        if (!_store.IsConfigured || !await _store.IsSchemaReadyAsync(ct))
            return new RulePromotionRunResult { Message = "RayvarzRuleEngine not ready" };

        if (!EnableAutoPromote && !forcePromote)
            return new RulePromotionRunResult { Message = "EnableAutoPromote=false — use POST /api/rule/promote/run?force=true for manual" };

        var steps = new List<string>();
        var candidates = await _store.GetCandidatesByStatusesAsync(NidMember,
            new[] { RuleCandidateStatus.Parsed, RuleCandidateStatus.Validated, RuleCandidateStatus.DryRunPassed }, ct);

        if (candidates.Count == 0)
            return new RulePromotionRunResult { Message = "No candidates in Parsed/Validated/DryRunPassed" };

        foreach (var candidate in candidates.OrderByDescending(c => c.SourceModifyAt))
        {
            var result = await ProcessCandidateAsync(candidate, forcePromote, steps, ct);
            if (result.Promoted || result.AnyAction)
            {
                return new RulePromotionRunResult
                {
                    AnyAction = result.AnyAction,
                    Promoted = result.Promoted,
                    CandidateId = result.CandidateId,
                    SnapshotId = result.SnapshotId,
                    Message = result.Message,
                    Steps = steps
                };
            }
        }

        return new RulePromotionRunResult { AnyAction = steps.Count > 0, Steps = steps, Message = "No promotion" };
    }

    public async Task<RulePromotionRunResult> RollbackToLegacyAsync(string? reason = null, CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct)
            ?? new RuleSyncStateRow { NidMember = NidMember, NidClass = 360 };

        state.ActiveEngine = "Legacy";
        state.ActiveSnapshotId = null;
        state.ConsecutiveDynamicFailures = 0;
        state.CircuitBreakerOpenUntilUtc = null;
        await _store.UpsertSyncStateAsync(state, ct);
        await _store.DeactivateAllSnapshotsAsync(NidMember, ct);

        await _store.InsertPromotionLogAsync(NidMember, null, null, "RolledBack",
            reason ?? "Manual rollback to Legacy", ct);

        _logger.LogInformation("Rolled back to Legacy for NidMember {NidMember}", NidMember);
        return new RulePromotionRunResult { AnyAction = true, Message = "ActiveEngine=Legacy" };
    }

    private async Task<RulePromotionRunResult> ProcessCandidateAsync(
        RuleCandidateRow candidate, bool forcePromote, List<string> steps, CancellationToken ct)
    {
        var status = candidate.Status;
        var snapshot = await _store.GetSnapshotByHashAsync(NidMember, candidate.CanonicalXmlHash, ct);
        if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.DslJson))
        {
            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Rejected,
                "DSL snapshot missing", ct);
            steps.Add($"Candidate {candidate.CandidateId}: rejected — no snapshot");
            return new RulePromotionRunResult { AnyAction = true };
        }

        var program = RuleDslParserService.DeserializeProgram(snapshot.DslJson);
        if (program == null)
        {
            await RejectCandidate(candidate.CandidateId, "Invalid DslJson", ct);
            steps.Add($"Candidate {candidate.CandidateId}: invalid DslJson");
            return new RulePromotionRunResult { AnyAction = true };
        }

        if (status is RuleCandidateStatus.Parsed or RuleCandidateStatus.Detected)
        {
            var validation = _validator.Validate(program);
            if (!validation.Success)
            {
                await RejectCandidate(candidate.CandidateId, string.Join("; ", validation.Errors), ct);
                steps.Add($"Candidate {candidate.CandidateId}: validation failed");
                return new RulePromotionRunResult { AnyAction = true };
            }
            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Validated, ct: ct);
            await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "Validated", null, ct);
            status = RuleCandidateStatus.Validated;
            steps.Add($"Candidate {candidate.CandidateId}: Validated");
        }

        if (status == RuleCandidateStatus.Validated)
        {
            var dryRun = await _goldenDryRun.RunAllWithEngineAsync(
                _dynamic, candidate.CandidateId, snapshot.SnapshotId, compareExpectedRows: true,
                allowLegacyFallback: false, ct);
            if (!dryRun.AllPassed)
            {
                await RejectCandidate(candidate.CandidateId,
                    $"Golden dry-run failed: {dryRun.Passed}/{dryRun.Total}", ct);
                steps.Add($"Candidate {candidate.CandidateId}: golden failed {dryRun.Passed}/{dryRun.Total}");
                return new RulePromotionRunResult { AnyAction = true };
            }
            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.DryRunPassed, ct: ct);
            await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "DryRunPassed", null, ct);
            status = RuleCandidateStatus.DryRunPassed;
            steps.Add($"Candidate {candidate.CandidateId}: DryRunPassed 4/4");
        }

        if (!forcePromote && !IsStable(candidate))
        {
            steps.Add($"Candidate {candidate.CandidateId}: waiting stability until {candidate.StableEligibleAtUtc:u}");
            return new RulePromotionRunResult { AnyAction = true };
        }

        if (!await VerifyMemberHashAsync(candidate, ct))
        {
            await RejectCandidate(candidate.CandidateId, "CanonicalXmlHash != active Member.XmlBody", ct);
            steps.Add($"Candidate {candidate.CandidateId}: hash mismatch with Member");
            return new RulePromotionRunResult { AnyAction = true };
        }

        return await PromoteAsync(candidate, snapshot, steps, ct);
    }

    private bool IsStable(RuleCandidateRow candidate)
    {
        if (DateTime.UtcNow < candidate.StableEligibleAtUtc.ToUniversalTime())
            return false;
        return true;
    }

    private async Task<bool> VerifyMemberHashAsync(RuleCandidateRow candidate, CancellationToken ct)
    {
        var member = await _members.LoadActiveMemberAsync(NidMember, ct: ct);
        if (member == null || string.IsNullOrWhiteSpace(member.XmlBody))
            return false;
        try
        {
            var envelope = XmlEnvelopeReader.Read(member.XmlBody, member.Source);
            return envelope.XmlHash.Equals(candidate.CanonicalXmlHash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> VerifyNoNewerHistoryAsync(RuleCandidateRow candidate, CancellationToken ct)
    {
        var latest = await _members.LoadLatestHistoryAsync(NidMember, ct);
        return latest != null && latest.NidHistory == candidate.SourceNidHistory;
    }

    private async Task<RulePromotionRunResult> PromoteAsync(
        RuleCandidateRow candidate, RuleDslSnapshotRow snapshot, List<string> steps, CancellationToken ct)
    {
        if (!await VerifyNoNewerHistoryAsync(candidate, ct))
        {
            steps.Add($"Candidate {candidate.CandidateId}: newer MemberHistory exists");
            return new RulePromotionRunResult { AnyAction = true };
        }

        var state = await _store.GetSyncStateAsync(NidMember, ct)
            ?? new RuleSyncStateRow { NidMember = NidMember, NidClass = 360 };

        await _store.ActivateSnapshotAsync(NidMember, snapshot.SnapshotId, ct);
        await _store.SupersedeCandidatesAsync(NidMember, candidate.CandidateId, ct);
        await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Promoted, ct: ct);

        state.ActiveEngine = "Dynamic";
        state.ActiveSnapshotId = snapshot.SnapshotId;
        state.ActiveDslVersion = snapshot.DslVersion;
        state.LastStableNidHistory = candidate.SourceNidHistory;
        state.LastStableModifyAt = candidate.SourceModifyAt;
        state.LastStableXmlHash = candidate.CanonicalXmlHash;
        state.ConsecutiveDynamicFailures = 0;
        state.CircuitBreakerOpenUntilUtc = null;
        await _store.UpsertSyncStateAsync(state, ct);
        await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "Promoted",
            "ActiveEngine=Dynamic", ct);
        await _circuitBreaker.ResetAsync(ct);

        steps.Add($"PROMOTED candidate {candidate.CandidateId} snapshot {snapshot.SnapshotId}");
        _logger.LogInformation("Promoted candidate {CandidateId} — ActiveEngine=Dynamic DslVersion={Version}",
            candidate.CandidateId, snapshot.DslVersion);

        return new RulePromotionRunResult
        {
            AnyAction = true,
            Promoted = true,
            CandidateId = candidate.CandidateId,
            SnapshotId = snapshot.SnapshotId,
            Message = "ActiveEngine=Dynamic",
            Steps = steps
        };
    }

    private async Task RejectCandidate(long candidateId, string reason, CancellationToken ct)
    {
        await _store.UpdateCandidateStatusAsync(candidateId, RuleCandidateStatus.Rejected, reason, ct);
        await _store.InsertPromotionLogAsync(NidMember, candidateId, null, "Rejected", reason, ct);
    }
}

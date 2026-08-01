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
    private readonly RuleDslParserService _dslParser;
    private readonly ILogger<RulePromotionService> _logger;

    public RulePromotionService(
        IConfiguration config,
        RuleEngineStore store,
        MemberRuleRepository members,
        DslValidator validator,
        GoldenDryRunService goldenDryRun,
        DynamicRuleEngine dynamic,
        RuleCircuitBreakerService circuitBreaker,
        RuleDslParserService dslParser,
        ILogger<RulePromotionService> logger)
    {
        _config = config;
        _store = store;
        _members = members;
        _validator = validator;
        _goldenDryRun = goldenDryRun;
        _dynamic = dynamic;
        _circuitBreaker = circuitBreaker;
        _dslParser = dslParser;
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
            new[]
            {
                RuleCandidateStatus.Detected, RuleCandidateStatus.Parsing,
                RuleCandidateStatus.Parsed, RuleCandidateStatus.Validated, RuleCandidateStatus.DryRunPassed
            }, ct);

        if (candidates.Count == 0)
            return new RulePromotionRunResult { Message = "No candidates in Detected/Parsed/Validated/DryRunPassed" };

        foreach (var candidate in candidates.OrderByDescending(c => c.SourceModifyAt))
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Promotion failed for candidate {CandidateId}", candidate.CandidateId);
                steps.Add($"Candidate {candidate.CandidateId}: ERROR {ex.Message}");
                return new RulePromotionRunResult
                {
                    AnyAction = true,
                    Message = $"Promotion error: {ex.Message}",
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
            if (status is RuleCandidateStatus.Detected or RuleCandidateStatus.Parsing)
            {
                var parsed = await TryParseCandidateXmlAsync(candidate, steps, ct);
                if (parsed != null)
                {
                    snapshot = parsed;
                    status = RuleCandidateStatus.Parsed;
                }
            }

            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.DslJson))
            {
                await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Rejected,
                    "DSL snapshot missing", ct);
                steps.Add($"Candidate {candidate.CandidateId}: rejected — no snapshot");
                return new RulePromotionRunResult { AnyAction = true };
            }
        }

        if (status is RuleCandidateStatus.Detected or RuleCandidateStatus.Parsing)
        {
            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Parsed, ct: ct);
            await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "Parsed",
                "Snapshot linked during promote", ct);
            status = RuleCandidateStatus.Parsed;
            steps.Add($"Candidate {candidate.CandidateId}: Parsed (snapshot exists)");
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
            var validation = _validator.Validate(program, strictUnsupportedStatements: false);
            if (!validation.Success)
            {
                var reason = SummarizeErrors(validation.Errors);
                await RejectCandidate(candidate.CandidateId, reason, ct);
                steps.Add($"Candidate {candidate.CandidateId}: validation failed — {reason}");
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
                var failed = dryRun.Cases.Where(c => !c.Success).Take(3)
                    .Select(c => $"{c.FicheNo}: {c.ErrorMessage}");
                var reason = $"Golden dry-run failed: {dryRun.Passed}/{dryRun.Total} — {string.Join(" | ", failed)}";
                await RejectCandidate(candidate.CandidateId, reason, ct);
                steps.Add($"Candidate {candidate.CandidateId}: golden failed {dryRun.Passed}/{dryRun.Total}");
                foreach (var c in dryRun.Cases.Where(x => !x.Success))
                    steps.Add($"  {c.FicheNo}: {c.ErrorMessage}");
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

    private async Task<RuleDslSnapshotRow?> TryParseCandidateXmlAsync(
        RuleCandidateRow candidate, List<string> steps, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(candidate.XmlBody))
            return null;

        await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Parsing, ct: ct);
        var result = await _dslParser.ParseAndStoreAsync(candidate.XmlBody, "MemberHistory", ct);
        if (result.Parse?.Success != true)
        {
            await RejectCandidate(candidate.CandidateId, result.Parse?.ErrorMessage ?? result.Message ?? "Parse failed", ct);
            steps.Add($"Candidate {candidate.CandidateId}: parse failed");
            return null;
        }

        await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, result.SnapshotId, "Parsed",
            result.SkippedExisting ? "DSL snapshot already existed" : "DSL snapshot stored", ct);
        steps.Add($"Candidate {candidate.CandidateId}: parsed on demand");
        return await _store.GetSnapshotByHashAsync(NidMember, candidate.CanonicalXmlHash, ct);
    }

    private bool IsStable(RuleCandidateRow candidate)
    {
        if (DateTime.UtcNow < candidate.StableEligibleAtUtc.ToUniversalTime())
            return false;
        return true;
    }

    private async Task<bool> VerifyMemberHashAsync(RuleCandidateRow candidate, CancellationToken ct)
    {
        try
        {
            var member = await _members.LoadActiveMemberAsync(NidMember, ct: ct);
            if (member == null || string.IsNullOrWhiteSpace(member.XmlBody))
                return false;

            var envelope = XmlEnvelopeReader.Read(member.XmlBody, member.Source);
            return envelope.XmlHash.Equals(candidate.CanonicalXmlHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "VerifyMemberHash failed for candidate {CandidateId}", candidate.CandidateId);
            return false;
        }
    }

    private async Task<bool> VerifyNoNewerHistoryAsync(RuleCandidateRow candidate, CancellationToken ct)
    {
        try
        {
            var latest = await _members.LoadLatestHistoryAsync(NidMember, ct);
            return latest != null && latest.NidHistory == candidate.SourceNidHistory;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "VerifyNoNewerHistory failed for candidate {CandidateId}", candidate.CandidateId);
            return false;
        }
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
        var trimmed = TruncateReason(reason);
        await _store.UpdateCandidateStatusAsync(candidateId, RuleCandidateStatus.Rejected, trimmed, ct);
        await _store.InsertPromotionLogAsync(NidMember, candidateId, null, "Rejected", trimmed, ct);
    }

    private static string SummarizeErrors(IReadOnlyList<string> errors)
    {
        if (errors.Count == 0) return "Validation failed";
        if (errors.Count == 1) return errors[0];
        return $"Validation failed ({errors.Count} errors): {string.Join("; ", errors.Take(3))}";
    }

    private static string TruncateReason(string? reason, int maxLen = 480) =>
        string.IsNullOrEmpty(reason) ? "" : reason.Length <= maxLen ? reason : reason[..maxLen];
}

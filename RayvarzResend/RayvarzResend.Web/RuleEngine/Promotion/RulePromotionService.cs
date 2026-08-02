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
        var rejected = await _store.GetCandidatesByStatusesAsync(NidMember,
            new[] { RuleCandidateStatus.Rejected }, ct);
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
            Candidates = candidates.Select(MapCandidate).ToList(),
            RejectedCandidates = rejected.Select(MapCandidate).ToList(),
            RecentLogs = logs
        };
    }

    public async Task<RulePromotionRunResult> EvaluatePromotionsAsync(bool forcePromote = false, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Promote START NidMember={NidMember} force={Force} EnableAutoPromote={Auto}",
            NidMember, forcePromote, EnableAutoPromote);

        if (!_store.IsConfigured || !await _store.IsSchemaReadyAsync(ct))
        {
            _logger.LogWarning("Promote aborted: RayvarzRuleEngine not ready");
            return new RulePromotionRunResult { Message = "RayvarzRuleEngine not ready", FailedStage = "Schema" };
        }

        if (!EnableAutoPromote && !forcePromote)
            return new RulePromotionRunResult
            {
                Message = "EnableAutoPromote=false — use POST /api/rule/promote/run?force=true for manual",
                FailedStage = "Config"
            };

        var steps = new List<string>();
        var statusFilter = new List<string>
        {
            RuleCandidateStatus.Detected, RuleCandidateStatus.Parsing,
            RuleCandidateStatus.Parsed, RuleCandidateStatus.Validated, RuleCandidateStatus.DryRunPassed
        };
        // force: Rejected را هم دوباره امتحان کن (بدون نیاز به SQL دستی)
        if (forcePromote)
            statusFilter.Add(RuleCandidateStatus.Rejected);

        var candidates = await _store.GetCandidatesByStatusesAsync(NidMember, statusFilter, ct);
        var statusSnapshot = candidates.Select(MapCandidate).ToList();

        _logger.LogInformation(
            "Promote candidates found={Count}: {Statuses}",
            candidates.Count,
            string.Join(", ", candidates.Select(c => $"{c.CandidateId}:{c.Status}")));

        if (candidates.Count == 0)
        {
            var rejected = await _store.GetCandidatesByStatusesAsync(NidMember,
                new[] { RuleCandidateStatus.Rejected }, ct);
            steps.Add($"No eligible candidates. RejectedCount={rejected.Count}");
            foreach (var r in rejected.Take(5))
                steps.Add($"  Rejected {r.CandidateId}: {r.RejectReason}");

            _logger.LogWarning("Promote: no candidates. Rejected={Count}", rejected.Count);
            return new RulePromotionRunResult
            {
                Message = forcePromote
                    ? "No candidates (even Rejected). Run sync/run then dsl/parse first."
                    : "No candidates in Detected/Parsed/Validated/DryRunPassed — check Rejected or use force=true",
                FailedStage = "NoCandidate",
                Steps = steps,
                CandidateStatuses = rejected.Select(MapCandidate).ToList()
            };
        }

        foreach (var item in candidates.OrderByDescending(c => c.SourceModifyAt))
        {
            var candidate = item;
            try
            {
                if (forcePromote && candidate.Status == RuleCandidateStatus.Rejected)
                {
                    _logger.LogInformation("Force-reset Rejected candidate {CandidateId} → Parsed", candidate.CandidateId);
                    await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Parsed, null, ct);
                    steps.Add($"Candidate {candidate.CandidateId}: force reset Rejected → Parsed");
                    candidate = CloneWithStatus(candidate, RuleCandidateStatus.Parsed);
                }

                var result = await ProcessCandidateAsync(candidate, forcePromote, steps, ct);
                if (result.Promoted || result.AnyAction)
                {
                    _logger.LogInformation(
                        "Promote END candidate={Id} promoted={Promoted} stage={Stage} message={Message}",
                        result.CandidateId ?? candidate.CandidateId, result.Promoted,
                        result.FailedStage ?? "done", result.Message);
                    return new RulePromotionRunResult
                    {
                        AnyAction = result.AnyAction,
                        Promoted = result.Promoted,
                        CandidateId = result.CandidateId ?? candidate.CandidateId,
                        SnapshotId = result.SnapshotId,
                        Message = result.Message,
                        FailedStage = result.FailedStage,
                        Steps = steps,
                        ValidationErrors = result.ValidationErrors,
                        UnknownOperations = result.UnknownOperations,
                        GoldenFailures = result.GoldenFailures,
                        CandidateStatuses = statusSnapshot
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Promotion exception candidate={CandidateId}", candidate.CandidateId);
                steps.Add($"Candidate {candidate.CandidateId}: EXCEPTION {ex.GetType().Name}: {ex.Message}");
                return new RulePromotionRunResult
                {
                    AnyAction = true,
                    CandidateId = candidate.CandidateId,
                    Message = $"Promotion error: {ex.Message}",
                    FailedStage = "Exception",
                    Steps = steps,
                    CandidateStatuses = statusSnapshot
                };
            }
        }

        return new RulePromotionRunResult
        {
            AnyAction = steps.Count > 0,
            Steps = steps,
            Message = "No promotion",
            FailedStage = "None",
            CandidateStatuses = statusSnapshot
        };
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
        return new RulePromotionRunResult { AnyAction = true, Message = "ActiveEngine=Legacy", FailedStage = null };
    }

    private async Task<RulePromotionRunResult> ProcessCandidateAsync(
        RuleCandidateRow candidate, bool forcePromote, List<string> steps, CancellationToken ct)
    {
        var status = candidate.Status;
        _logger.LogInformation(
            "ProcessCandidate id={Id} status={Status} hash={Hash} history={Hist}",
            candidate.CandidateId, status,
            candidate.CanonicalXmlHash[..Math.Min(12, candidate.CanonicalXmlHash.Length)],
            candidate.SourceNidHistory);

        var snapshot = await _store.GetSnapshotByHashAsync(NidMember, candidate.CanonicalXmlHash, ct);
        if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.DslJson))
        {
            _logger.LogWarning("Candidate {Id}: snapshot missing for hash — try parse", candidate.CandidateId);
            if (status is RuleCandidateStatus.Detected or RuleCandidateStatus.Parsing or RuleCandidateStatus.Parsed)
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
                await RejectCandidate(candidate.CandidateId, "DSL snapshot missing", ct);
                steps.Add($"Candidate {candidate.CandidateId}: rejected — no snapshot (hash={candidate.CanonicalXmlHash[..12]})");
                return Fail(candidate.CandidateId, "Parse", "DSL snapshot missing", steps);
            }
        }

        steps.Add($"Candidate {candidate.CandidateId}: snapshotId={snapshot.SnapshotId} dslVersion={snapshot.DslVersion}");

        if (status is RuleCandidateStatus.Detected or RuleCandidateStatus.Parsing)
        {
            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Parsed, ct: ct);
            await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "Parsed",
                "Snapshot linked during promote", ct);
            status = RuleCandidateStatus.Parsed;
            steps.Add($"Candidate {candidate.CandidateId}: Parsed (snapshot exists)");
        }

        var program = RuleDslParserService.DeserializeProgram(snapshot.DslJson);
        var liveParse = _dslParser.Parse(candidate.XmlBody, "MemberHistory");
        if (liveParse.Success && liveParse.Program != null)
        {
            program = liveParse.Program;
            var freshJson = RuleDslParserService.SerializeProgram(program);
            if (!string.Equals(freshJson, snapshot.DslJson, StringComparison.Ordinal))
            {
                await _store.UpdateDslSnapshotAsync(snapshot.SnapshotId, freshJson, RuleDslParserService.ParserVersion, program.EntryPoint, ct);
                steps.Add($"Candidate {candidate.CandidateId}: snapshot DslJson refreshed ({freshJson.Length} chars)");
                _logger.LogInformation("Candidate {Id}: refreshed DSL snapshot ({Len} chars)", candidate.CandidateId, freshJson.Length);
            }
        }
        else if (!liveParse.Success)
        {
            _logger.LogWarning("Candidate {Id}: live re-parse failed: {Err}", candidate.CandidateId, liveParse.ErrorMessage);
            steps.Add($"Candidate {candidate.CandidateId}: live re-parse warning: {liveParse.ErrorMessage}");
        }

        if (program == null)
        {
            await RejectCandidate(candidate.CandidateId, "Invalid DslJson", ct);
            return Fail(candidate.CandidateId, "Parse", "Invalid DslJson", steps);
        }

        if (status is RuleCandidateStatus.Parsed or RuleCandidateStatus.Detected)
        {
            var validation = _validator.ValidateForPromotion(program);
            _logger.LogInformation(
                "Candidate {Id} ValidateForPromotion success={Ok} errors={ErrCount} unknownOps=[{Ops}] warnings={WarnCount}",
                candidate.CandidateId, validation.Success, validation.Errors.Count,
                string.Join(", ", validation.UnknownOperations.Take(20)),
                validation.Warnings.Count);

            if (!validation.Success)
            {
                var reason = SummarizeErrors(validation.Errors);
                await RejectCandidate(candidate.CandidateId, reason, ct);
                steps.Add($"Candidate {candidate.CandidateId}: validation FAILED");
                foreach (var err in validation.Errors.Take(20))
                    steps.Add($"  ERROR: {err}");
                foreach (var op in validation.UnknownOperations.Take(20))
                    steps.Add($"  UNKNOWN_OP: {op}");

                return new RulePromotionRunResult
                {
                    AnyAction = true,
                    CandidateId = candidate.CandidateId,
                    SnapshotId = snapshot.SnapshotId,
                    Message = reason,
                    FailedStage = "Validate",
                    Steps = steps,
                    ValidationErrors = validation.Errors.ToList(),
                    UnknownOperations = validation.UnknownOperations.ToList()
                };
            }

            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Validated, ct: ct);
            await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "Validated", null, ct);
            status = RuleCandidateStatus.Validated;
            steps.Add($"Candidate {candidate.CandidateId}: Validated OK (unknownOps ignored/known={validation.UnknownOperations.Count})");
        }

        if (status == RuleCandidateStatus.Validated)
        {
            _logger.LogInformation("Candidate {Id}: starting golden dry-run (Dynamic, no Legacy fallback)", candidate.CandidateId);
            steps.Add($"Candidate {candidate.CandidateId}: golden dry-run starting…");

            var dryRun = await _goldenDryRun.RunAllWithEngineAsync(
                _dynamic, candidate.CandidateId, snapshot.SnapshotId, compareExpectedRows: true,
                allowLegacyFallback: false, ct);

            _logger.LogInformation(
                "Candidate {Id}: golden result passed={Passed}/{Total} engine={Engine}",
                candidate.CandidateId, dryRun.Passed, dryRun.Total, dryRun.EngineName);

            if (!dryRun.AllPassed)
            {
                var goldenFails = dryRun.Cases.Where(c => !c.Success).Select(c => (object)new
                {
                    c.FicheNo,
                    c.Name,
                    c.ErrorMessage,
                    c.RowCount,
                    c.Payable,
                    c.RowSum,
                    mismatches = c.Mismatches
                }).ToList();

                var failed = dryRun.Cases.Where(c => !c.Success).Take(3)
                    .Select(c => $"{c.FicheNo}: {c.ErrorMessage}");
                var reason = $"Golden dry-run failed: {dryRun.Passed}/{dryRun.Total} — {string.Join(" | ", failed)}";
                await RejectCandidate(candidate.CandidateId, reason, ct);
                steps.Add($"Candidate {candidate.CandidateId}: golden FAILED {dryRun.Passed}/{dryRun.Total} engine={dryRun.EngineName}");
                foreach (var c in dryRun.Cases)
                    steps.Add($"  GOLDEN {(c.Success ? "OK" : "FAIL")} {c.FicheNo}: {c.ErrorMessage ?? "pass"}");

                return new RulePromotionRunResult
                {
                    AnyAction = true,
                    CandidateId = candidate.CandidateId,
                    SnapshotId = snapshot.SnapshotId,
                    Message = reason,
                    FailedStage = "GoldenDryRun",
                    Steps = steps,
                    GoldenFailures = goldenFails
                };
            }

            await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.DryRunPassed, ct: ct);
            await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, snapshot.SnapshotId, "DryRunPassed", null, ct);
            status = RuleCandidateStatus.DryRunPassed;
            steps.Add($"Candidate {candidate.CandidateId}: DryRunPassed {dryRun.Passed}/{dryRun.Total}");
        }

        if (!forcePromote && !IsStable(candidate))
        {
            steps.Add($"Candidate {candidate.CandidateId}: waiting stability until {candidate.StableEligibleAtUtc:u}");
            _logger.LogInformation("Candidate {Id}: waiting stability until {Until:u}", candidate.CandidateId, candidate.StableEligibleAtUtc);
            return new RulePromotionRunResult
            {
                AnyAction = true,
                CandidateId = candidate.CandidateId,
                Message = "Waiting stability window",
                FailedStage = "Stability",
                Steps = steps
            };
        }

        if (!await VerifyMemberHashAsync(candidate, ct))
        {
            await RejectCandidate(candidate.CandidateId, "CanonicalXmlHash != active Member.XmlBody", ct);
            steps.Add($"Candidate {candidate.CandidateId}: HASH MISMATCH with active Member.XmlBody");
            _logger.LogWarning("Candidate {Id}: hash mismatch with Member", candidate.CandidateId);
            return Fail(candidate.CandidateId, "Hash", "CanonicalXmlHash != active Member.XmlBody", steps, snapshot.SnapshotId);
        }

        steps.Add($"Candidate {candidate.CandidateId}: hash OK — promoting…");
        return await PromoteAsync(candidate, snapshot, steps, ct);
    }

    private async Task<RuleDslSnapshotRow?> TryParseCandidateXmlAsync(
        RuleCandidateRow candidate, List<string> steps, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(candidate.XmlBody))
            return null;

        await _store.UpdateCandidateStatusAsync(candidate.CandidateId, RuleCandidateStatus.Parsing, ct: ct);
        var result = await _dslParser.ParseAndStoreAsync(candidate.XmlBody, "MemberHistory", ct: ct);
        if (result.Parse?.Success != true)
        {
            await RejectCandidate(candidate.CandidateId, result.Parse?.ErrorMessage ?? result.Message ?? "Parse failed", ct);
            steps.Add($"Candidate {candidate.CandidateId}: parse failed — {result.Parse?.ErrorMessage ?? result.Message}");
            _logger.LogWarning("Candidate {Id} parse failed: {Msg}", candidate.CandidateId, result.Message);
            return null;
        }

        await _store.InsertPromotionLogAsync(NidMember, candidate.CandidateId, result.SnapshotId, "Parsed",
            result.SkippedExisting ? "DSL snapshot already existed" : "DSL snapshot stored", ct);
        steps.Add($"Candidate {candidate.CandidateId}: parsed on demand snapshotId={result.SnapshotId}");
        return await _store.GetSnapshotByHashAsync(NidMember, candidate.CanonicalXmlHash, ct);
    }

    private bool IsStable(RuleCandidateRow candidate) =>
        DateTime.UtcNow >= candidate.StableEligibleAtUtc.ToUniversalTime();

    private async Task<bool> VerifyMemberHashAsync(RuleCandidateRow candidate, CancellationToken ct)
    {
        try
        {
            var member = await _members.LoadActiveMemberAsync(NidMember, ct: ct);
            if (member == null || string.IsNullOrWhiteSpace(member.XmlBody))
            {
                _logger.LogWarning("VerifyMemberHash: Member XmlBody missing");
                return false;
            }

            var envelope = XmlEnvelopeReader.Read(member.XmlBody, member.Source);
            var ok = envelope.XmlHash.Equals(candidate.CanonicalXmlHash, StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation(
                "VerifyMemberHash candidate={Cand} member={Mem} match={Ok}",
                candidate.CanonicalXmlHash[..12], envelope.XmlHash[..12], ok);
            return ok;
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
            var ok = latest != null && latest.NidHistory == candidate.SourceNidHistory;
            _logger.LogInformation(
                "VerifyNoNewerHistory candidateHist={Cand} latestHist={Latest} ok={Ok}",
                candidate.SourceNidHistory, latest?.NidHistory, ok);
            return ok;
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
            steps.Add($"Candidate {candidate.CandidateId}: newer MemberHistory exists — blocked");
            return Fail(candidate.CandidateId, "History", "Newer MemberHistory exists", steps, snapshot.SnapshotId);
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
        _logger.LogInformation("PROMOTED candidate {CandidateId} — ActiveEngine=Dynamic DslVersion={Version}",
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
        _logger.LogWarning("Reject candidate {Id}: {Reason}", candidateId, trimmed);
        await _store.UpdateCandidateStatusAsync(candidateId, RuleCandidateStatus.Rejected, trimmed, ct);
        await _store.InsertPromotionLogAsync(NidMember, candidateId, null, "Rejected", trimmed, ct);
    }

    private static RulePromotionRunResult Fail(
        long? candidateId, string stage, string message, List<string> steps, long? snapshotId = null) =>
        new()
        {
            AnyAction = true,
            CandidateId = candidateId,
            SnapshotId = snapshotId,
            Message = message,
            FailedStage = stage,
            Steps = steps
        };

    private static object MapCandidate(RuleCandidateRow c) => new
    {
        c.CandidateId,
        c.Status,
        c.SourceNidHistory,
        c.SourceModifyAt,
        c.StableEligibleAtUtc,
        c.RejectReason,
        hashPrefix = c.CanonicalXmlHash[..Math.Min(12, c.CanonicalXmlHash.Length)]
    };

    private static RuleCandidateRow CloneWithStatus(RuleCandidateRow c, string status) =>
        new()
        {
            CandidateId = c.CandidateId,
            NidMember = c.NidMember,
            SourceNidHistory = c.SourceNidHistory,
            SourceModifyAt = c.SourceModifyAt,
            CanonicalXmlHash = c.CanonicalXmlHash,
            XmlBody = c.XmlBody,
            Modifyer = c.Modifyer,
            ModifyDesc = c.ModifyDesc,
            Status = status,
            RejectReason = null,
            StableEligibleAtUtc = c.StableEligibleAtUtc
        };

    private static string SummarizeErrors(IReadOnlyList<string> errors)
    {
        if (errors.Count == 0) return "Validation failed";
        if (errors.Count == 1) return errors[0];
        return $"Validation failed ({errors.Count} errors): {string.Join("; ", errors.Take(5))}";
    }

    private static string TruncateReason(string? reason, int maxLen = 480) =>
        string.IsNullOrEmpty(reason) ? "" : reason.Length <= maxLen ? reason : reason[..maxLen];
}

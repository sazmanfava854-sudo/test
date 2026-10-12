using Microsoft.Extensions.Logging;

namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>Phase 0–2 placeholder: records dry-run row; extend with golden fiche execution later.</summary>
public sealed class RuleGoldenDryRunService
{
    private readonly RuleEngineStateRepository _state;
    private readonly ILogger<RuleGoldenDryRunService> _logger;

    public RuleGoldenDryRunService(
        RuleEngineStateRepository state,
        ILogger<RuleGoldenDryRunService> logger)
    {
        _state = state;
        _logger = logger;
    }

    public async Task<DryRunOutcome> RunForCandidateAsync(RuleCandidateRow candidate, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(candidate.XmlBody))
        {
            var msg = "Empty XmlBody on candidate";
            await _state.InsertDryRunResultAsync(candidate.CandidateId, null, null, "Placeholder", false, msg, null, ct);
            return new DryRunOutcome(false, msg, null);
        }

        _logger.LogInformation(
            "Dry-run placeholder OK for candidate {CandidateId} (golden execution not wired yet)",
            candidate.CandidateId);

        await _state.InsertDryRunResultAsync(
            candidate.CandidateId,
            null,
            null,
            "Placeholder",
            true,
            null,
            """{"phase":"0-2","note":"schema-aligned stub"}""",
            ct);

        return new DryRunOutcome(true, null, null);
    }
}

public sealed record DryRunOutcome(bool Success, string? ErrorMessage, long? SnapshotId);

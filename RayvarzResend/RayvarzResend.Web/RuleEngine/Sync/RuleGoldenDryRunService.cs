namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>اسکلت Dry Run golden — فاز ۵ جزئیات assert را کامل می‌کند.</summary>
public sealed class RuleGoldenDryRunService
{
    private readonly RuleEngineStateRepository _state;

    public RuleGoldenDryRunService(RuleEngineStateRepository state) => _state = state;

    public async Task<(bool AllPassed, string Summary)> RunStructuralPlaceholderAsync(
        Guid? candidateId,
        int nidMember,
        CancellationToken ct = default)
    {
        var count = await _state.CountActiveGoldensAsync(ct);
        if (count == 0)
        {
            await _state.InsertDryRunResultAsync(candidateId, nidMember, null, true,
                "هیچ RuleGoldenFiche فعالی نیست — dry run ساختاری رد نشد.", ct);
            return (true, "no golden rows — skipped");
        }

        var passed = true;
        await _state.InsertDryRunResultAsync(candidateId, nidMember, null, passed,
            $"placeholder: {count} golden fiche(s) registered; full Legacy vs Dynamic compare در فاز ۵.", ct);
        return (passed, $"{count} golden(s) placeholder ok");
    }
}

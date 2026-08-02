using RayvarzResend.Web.RuleEngine.Engines;
using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Promotion;

/// <summary>فاز ۴: خطای متوالی Dynamic → بازگشت موقت به Legacy.</summary>
public sealed class RuleCircuitBreakerService
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly ILogger<RuleCircuitBreakerService> _logger;

    public RuleCircuitBreakerService(IConfiguration config, RuleEngineStore store, ILogger<RuleCircuitBreakerService> logger)
    {
        _config = config;
        _store = store;
        _logger = logger;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    private int FailureThreshold => _config.GetValue("RuleEngine:CircuitBreakerFailureThreshold", 3);
    private int CooldownMinutes => _config.GetValue("RuleEngine:CircuitBreakerCooldownMinutes", 60);

    public async Task<bool> IsOpenAsync(CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct);
        if (state == null) return false;
        if (!string.Equals(state.ActiveEngine, "Dynamic", StringComparison.OrdinalIgnoreCase)
            && state.CircuitBreakerOpenUntilUtc == null)
            return false;

        if (state.CircuitBreakerOpenUntilUtc is { } until && until > DateTime.UtcNow)
            return true;

        if (state.CircuitBreakerOpenUntilUtc is { } expired && expired <= DateTime.UtcNow)
            await ResetAsync(ct);

        return false;
    }

    public async Task RecordDynamicSuccessAsync(CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct);
        if (state == null) return;
        if (state.ConsecutiveDynamicFailures == 0 && state.CircuitBreakerOpenUntilUtc == null)
            return;

        state.ConsecutiveDynamicFailures = 0;
        state.CircuitBreakerOpenUntilUtc = null;
        await _store.UpsertSyncStateAsync(state, ct);
    }

    public async Task RecordDynamicFailureAsync(string reason, CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct)
            ?? new RuleSyncStateRow { NidMember = NidMember, NidClass = 360, ActiveEngine = "Legacy" };

        state.ConsecutiveDynamicFailures++;
        if (state.ConsecutiveDynamicFailures < FailureThreshold)
        {
            await _store.UpsertSyncStateAsync(state, ct);
            return;
        }

        state.ActiveEngine = "Legacy";
        state.CircuitBreakerOpenUntilUtc = DateTime.UtcNow.AddMinutes(CooldownMinutes);
        await _store.UpsertSyncStateAsync(state, ct);
        await _store.InsertPromotionLogAsync(NidMember, null, state.ActiveSnapshotId, "CircuitBreakerOpen", reason, ct);
        _logger.LogWarning(
            "Circuit breaker OPEN — Dynamic disabled until {Until:u} after {Failures} failures",
            state.CircuitBreakerOpenUntilUtc, state.ConsecutiveDynamicFailures);
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        var state = await _store.GetSyncStateAsync(NidMember, ct);
        if (state == null) return;
        state.ConsecutiveDynamicFailures = 0;
        state.CircuitBreakerOpenUntilUtc = null;
        await _store.UpsertSyncStateAsync(state, ct);
    }
}

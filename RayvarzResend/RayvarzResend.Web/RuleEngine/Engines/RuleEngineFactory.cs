using RayvarzResend.Web.RuleEngine.Promotion;
using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Engines;

public sealed class RuleEngineFactory
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly LegacyRuleEngine _legacy;
    private readonly DynamicRuleEngine _dynamic;
    private readonly RuleCircuitBreakerService _circuitBreaker;

    public RuleEngineFactory(
        IConfiguration config,
        RuleEngineStore store,
        LegacyRuleEngine legacy,
        DynamicRuleEngine dynamic,
        RuleCircuitBreakerService circuitBreaker)
    {
        _config = config;
        _store = store;
        _legacy = legacy;
        _dynamic = dynamic;
        _circuitBreaker = circuitBreaker;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);

    /// <summary>موتور فعال بر اساس RuleSyncState.ActiveEngine یا ForceEngine در config.</summary>
    public async Task<IFicheRuleEngine> ResolveAsync(CancellationToken ct = default)
    {
        var forced = (_config["RuleEngine:ForceEngine"] ?? "").Trim();
        if (!string.IsNullOrEmpty(forced))
        {
            if (await _circuitBreaker.IsOpenAsync(ct))
                return _legacy;
            return ResolveByName(forced);
        }

        if (await _circuitBreaker.IsOpenAsync(ct))
            return _legacy;

        if (_store.IsConfigured && await _store.IsSchemaReadyAsync(ct))
        {
            var state = await _store.GetSyncStateAsync(NidMember, ct);
            if (!string.IsNullOrWhiteSpace(state?.ActiveEngine))
                return ResolveByName(state.ActiveEngine);
        }

        return _legacy;
    }

    public async Task<string> ResolveEngineNameAsync(CancellationToken ct = default)
    {
        var engine = await ResolveAsync(ct);
        return engine.EngineName;
    }

    public IFicheRuleEngine ResolveByName(string name) =>
        name.Trim().ToLowerInvariant() switch
        {
            "dynamic" => _dynamic,
            "legacy" or "legacycsharp" => _legacy,
            _ => _legacy
        };

    public LegacyRuleEngine Legacy => _legacy;
    public DynamicRuleEngine Dynamic => _dynamic;

    /// <summary>ارزیابی با موتور resolve‌شده؛ در صورت Dynamic و خطا، fallback به Legacy.</summary>
    public async Task<FicheRuleEvaluationResult> EvaluateWithFallbackAsync(
        FicheRuleContext context,
        bool buildSoap = false,
        CancellationToken ct = default)
    {
        var engine = await ResolveAsync(ct);
        var result = await engine.EvaluateAsync(context, buildSoap, ct);

        if (result.Success)
        {
            if (engine is DynamicRuleEngine && string.Equals(result.EngineName, "Dynamic", StringComparison.OrdinalIgnoreCase))
                await _circuitBreaker.RecordDynamicSuccessAsync(ct);
            return result;
        }

        if (engine is not DynamicRuleEngine)
            return result;

        await _circuitBreaker.RecordDynamicFailureAsync(result.ErrorMessage ?? "Dynamic evaluation failed", ct);

        if (!context.AllowLegacyFallback || !_config.GetValue("RuleEngine:DynamicFallbackToLegacy", true))
            return result;

        return await _legacy.EvaluateAsync(context, buildSoap, ct);
    }
}

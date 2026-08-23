using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Engines;

public sealed class RuleEngineFactory
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly LegacyRuleEngine _legacy;
    private readonly DynamicRuleEngine _dynamic;

    public RuleEngineFactory(
        IConfiguration config,
        RuleEngineStore store,
        LegacyRuleEngine legacy,
        DynamicRuleEngine dynamic)
    {
        _config = config;
        _store = store;
        _legacy = legacy;
        _dynamic = dynamic;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);

    /// <summary>موتور فعال بر اساس RuleSyncState.ActiveEngine یا ForceEngine در config.</summary>
    public async Task<IFicheRuleEngine> ResolveAsync(CancellationToken ct = default)
    {
        var forced = (_config["RuleEngine:ForceEngine"] ?? "").Trim();
        if (!string.IsNullOrEmpty(forced))
            return ResolveByName(forced);

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
}

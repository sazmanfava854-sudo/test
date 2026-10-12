using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.RuleEngine.Sync;

public interface IRuleSendGateStore
{
    bool IsConfigured { get; }
    Task<string?> GetSendGateAsync(int nidMember, CancellationToken ct = default);
}

public sealed class RuleSendGateBlockedException : InvalidOperationException
{
    public RuleSendGateBlockedException() : base(RuleSendGate.BlockedMessage)
    {
    }
}

/// <summary>
/// بررسی RuleSyncState.SendGate قبل از هر ارسال به رایورز.
/// خطای دسترسی به RayvarzRuleEngine ارسال را متوقف نمی‌کند (fail-open + هشدار در لاگ).
/// </summary>
public sealed class RuleSendGate
{
    public const string BlockedMessage = "تغییر ایجاد شده";

    private readonly IRuleSendGateStore _store;
    private readonly RuleEngineSyncOptions _options;
    private readonly ILogger<RuleSendGate> _logger;
    private readonly Func<DateTime> _utcNow;
    private CachedGate? _cached;

    public RuleSendGate(IRuleSendGateStore store, IOptions<RuleEngineSyncOptions> options, ILogger<RuleSendGate> logger)
        : this(store, options.Value, logger, () => DateTime.UtcNow)
    {
    }

    internal RuleSendGate(IRuleSendGateStore store, RuleEngineSyncOptions options, ILogger<RuleSendGate> logger, Func<DateTime> utcNow)
    {
        _store = store;
        _options = options;
        _logger = logger;
        _utcNow = utcNow;
    }

    public bool IsActive => _options.Enabled && _options.SendGateEnabled && _store.IsConfigured;

    public async Task<bool> IsBlockedAsync(CancellationToken ct = default)
    {
        if (!IsActive)
            return false;

        var now = _utcNow();
        var cached = _cached;
        if (cached != null && now < cached.ExpiresAtUtc)
            return cached.Blocked;

        bool blocked;
        try
        {
            blocked = RuleSendGateStates.IsBlocked(await _store.GetSendGateAsync(_options.NidMember, ct));
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Rule send gate could not be read from RayvarzRuleEngine — sending allowed");
            blocked = false;
        }

        _cached = new CachedGate(blocked, now.AddSeconds(Math.Max(0, _options.SendGateCacheSeconds)));
        return blocked;
    }

    public async Task EnsureOpenAsync(CancellationToken ct = default)
    {
        if (await IsBlockedAsync(ct))
            throw new RuleSendGateBlockedException();
    }

    public void Invalidate() => _cached = null;

    private sealed record CachedGate(bool Blocked, DateTime ExpiresAtUtc);
}

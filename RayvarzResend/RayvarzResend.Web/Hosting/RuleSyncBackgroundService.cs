using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.RuleEngine.Promotion;
using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.Hosting;

public sealed class RuleSyncBackgroundService : BackgroundService
{
    private readonly RuleVersionManager _versionManager;
    private readonly RulePromotionService _promotion;
    private readonly IConfiguration _config;
    private readonly ILogger<RuleSyncBackgroundService> _logger;

    public RuleSyncBackgroundService(
        RuleVersionManager versionManager,
        RulePromotionService promotion,
        IConfiguration config,
        ILogger<RuleSyncBackgroundService> logger)
    {
        _versionManager = versionManager;
        _promotion = promotion;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.GetValue("RuleEngine:EnableBackgroundSync", true))
            return;

        var minutes = _config.GetValue("RuleEngine:PollIntervalMinutes", 15);
        try
        {
            await _versionManager.InitializeAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rule sync initial run failed");
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, minutes)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await _versionManager.EvaluateChangesAsync(ct: stoppingToken);
                var promote = await _promotion.EvaluatePromotionsAsync(ct: stoppingToken);
                if (promote.Promoted)
                    _logger.LogInformation("Rule promotion: {Message}", promote.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Rule sync poll failed");
            }
        }
    }
}

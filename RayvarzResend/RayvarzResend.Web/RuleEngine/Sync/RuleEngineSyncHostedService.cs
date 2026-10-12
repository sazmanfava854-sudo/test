using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleEngineSyncHostedService : BackgroundService
{
    private readonly RuleEngineSyncOptions _options;
    private readonly RuleVersionManager _manager;
    private readonly ILogger<RuleEngineSyncHostedService> _logger;

    public RuleEngineSyncHostedService(
        IOptions<RuleEngineSyncOptions> options,
        RuleVersionManager manager,
        ILogger<RuleEngineSyncHostedService> logger)
    {
        _options = options.Value;
        _manager = manager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("RuleEngineSyncHostedService: disabled");
            return;
        }

        var delay = TimeSpan.FromMinutes(Math.Max(1, _options.PollIntervalMinutes));
        _logger.LogInformation("RuleEngineSyncHostedService: poll every {Minutes} min, stability {Hours}h",
            _options.PollIntervalMinutes, _options.StabilityHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _manager.RunCycleAsync(stoppingToken);
                if (!result.Ok)
                    _logger.LogWarning("Rule sync cycle: {Error}", result.Error);
                else
                    _logger.LogDebug("Rule sync cycle: {Summary}", result.Summary);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Rule sync background cycle failed");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}

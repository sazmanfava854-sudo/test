using HRPerformance.Interfaces;
using HRPerformance.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRPerformance.BackgroundServices;

public class AttendanceSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AttendanceSyncBackgroundService> _logger;

    public AttendanceSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AttendanceSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue<bool>("HrIntegration:Enabled"))
        {
            _logger.LogInformation("HR integration background sync is disabled");
            return;
        }

        var intervalMinutes = _configuration.GetValue<int>("HrIntegration:SyncIntervalMinutes", 5);
        var startupDelaySeconds = _configuration.GetValue<int>("HrIntegration:SyncStartupDelaySeconds", 15);
        _logger.LogInformation(
            "HR integration sync scheduled: first run after {Delay}s, then every {Interval} min (mode={Mode})",
            startupDelaySeconds,
            intervalMinutes,
            _configuration["HrIntegration:SyncMode"] ?? "Monthly");
        await Task.Delay(TimeSpan.FromSeconds(startupDelaySeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var syncService = scope.ServiceProvider.GetRequiredService<IAttendanceSyncService>();

                if (!await context.Database.CanConnectAsync(stoppingToken))
                {
                    _logger.LogWarning("Database not available yet; skipping attendance sync");
                }
                else
                {
                    var orgIds = await context.AttendanceIntegrationSettings
                        .Where(s => s.IsActive)
                        .Select(s => s.OrganizationId)
                        .Distinct()
                        .ToListAsync(stoppingToken);

                    if (orgIds.Count == 0)
                    {
                        var defaultOrg = await context.Organizations.Select(o => o.Id).FirstOrDefaultAsync(stoppingToken);
                        if (defaultOrg != Guid.Empty)
                            orgIds.Add(defaultOrg);
                    }

                    foreach (var orgId in orgIds)
                        await syncService.SyncAsync(orgId, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background attendance sync error");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

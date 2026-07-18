using HRPerformance.Interfaces;
using HRPerformance.Data;
using HRPerformance.Services.ExternalHr;
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
            _logger.LogInformation("HR integration is disabled in appsettings");
            return;
        }

        _logger.LogInformation("HR integration background worker started. Sync runs only when enabled in admin settings.");
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalMinutes = 5;
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
                    var orgSettings = await context.AttendanceIntegrationSettings
                        .Where(s => s.IsActive && s.BackgroundSyncEnabled)
                        .ToListAsync(stoppingToken);

                    if (orgSettings.Count == 0)
                    {
                        _logger.LogDebug("No organization has background MIS sync enabled");
                    }
                    else
                    {
                        intervalMinutes = orgSettings.Min(s => Math.Max(1, s.SyncIntervalMinutes));
                        foreach (var setting in orgSettings)
                        {
                            _logger.LogInformation(
                                "Background MIS sync for org {OrgId} (mode={Mode}, employeeLimit={Limit})",
                                setting.OrganizationId,
                                setting.SyncMode,
                                setting.EmployeeLimit);

                            await syncService.SyncAsync(setting.OrganizationId, stoppingToken);
                        }
                    }
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

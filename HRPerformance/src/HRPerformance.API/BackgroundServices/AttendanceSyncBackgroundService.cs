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
        var intervalMinutes = _configuration.GetValue<int>("HrIntegration:SyncIntervalMinutes", 5);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var syncService = scope.ServiceProvider.GetRequiredService<IAttendanceSyncService>();

                var orgIds = await context.AttendanceIntegrationSettings
                    .Where(s => s.IsActive)
                    .Select(s => s.OrganizationId)
                    .Distinct()
                    .ToListAsync(stoppingToken);

                if (orgIds.Count == 0 && _configuration.GetValue<bool>("HrIntegration:Enabled"))
                {
                    var defaultOrg = await context.Organizations.Select(o => o.Id).FirstOrDefaultAsync(stoppingToken);
                    if (defaultOrg != Guid.Empty)
                        orgIds.Add(defaultOrg);
                }

                foreach (var orgId in orgIds)
                    await syncService.SyncAsync(orgId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background attendance sync error");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}

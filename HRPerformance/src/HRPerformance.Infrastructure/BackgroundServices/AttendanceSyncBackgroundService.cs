using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.BackgroundServices;
public class AttendanceSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AttendanceSyncBackgroundService> _logger;
    public AttendanceSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AttendanceSyncBackgroundService> logger)
    { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var syncService = scope.ServiceProvider.GetRequiredService<IAttendanceSyncService>();
                var orgs = await context.AttendanceIntegrationSettings.Where(s => s.IsActive).Select(s => s.OrganizationId).Distinct().ToListAsync(stoppingToken);
                foreach (var orgId in orgs) await syncService.SyncAsync(orgId, stoppingToken);
            }
            catch (Exception ex) { _logger.LogError(ex, "Background attendance sync error"); }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

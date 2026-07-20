using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;

/// <summary>
/// Applies idempotent SQL schema patches on startup (migrations 13 + 16).
/// </summary>
public static class DatabaseSchemaBootstrap
{
    private static readonly string[] SchemaPatches =
    [
        """
        IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'ProvinceCode') IS NULL
            ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [ProvinceCode] NVARCHAR(20) NOT NULL
                CONSTRAINT [DF_AttendanceIntegrationSettings_ProvinceCode] DEFAULT '147';
        """,
        """
        IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'ApplyProvinceFilter') IS NULL
            ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [ApplyProvinceFilter] BIT NOT NULL
                CONSTRAINT [DF_AttendanceIntegrationSettings_ApplyProvince] DEFAULT 1;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'LastEmployeeRosterSyncAt') IS NULL
            ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [LastEmployeeRosterSyncAt] DATETIME2 NULL;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'IsRosterSyncRunning') IS NULL
            ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [IsRosterSyncRunning] BIT NOT NULL
                CONSTRAINT [DF_AttendanceIntegrationSettings_RosterRunning] DEFAULT 0;
        """,
        """
        IF COL_LENGTH('dbo.Employees', 'LastSeenInRosterSyncAt') IS NULL
            ALTER TABLE [dbo].[Employees] ADD [LastSeenInRosterSyncAt] DATETIME2 NULL;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceSyncLogs', 'SyncType') IS NULL
            ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [SyncType] NVARCHAR(50) NULL;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceSyncLogs', 'EmployeesInserted') IS NULL
            ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [EmployeesInserted] INT NOT NULL
                CONSTRAINT [DF_AttendanceSyncLogs_EmpInserted] DEFAULT 0;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceSyncLogs', 'EmployeesUpdated') IS NULL
            ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [EmployeesUpdated] INT NOT NULL
                CONSTRAINT [DF_AttendanceSyncLogs_EmpUpdated] DEFAULT 0;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceSyncLogs', 'RequestedByUserName') IS NULL
            ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [RequestedByUserName] NVARCHAR(256) NULL;
        """,
        """
        IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'IsRosterSyncRunning') IS NOT NULL
            UPDATE [dbo].[AttendanceIntegrationSettings] SET [IsRosterSyncRunning] = 0 WHERE [IsRosterSyncRunning] = 1;
        """,
        """
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_Employees_Org_PersonnelCode_Active'
              AND object_id = OBJECT_ID(N'dbo.Employees'))
        BEGIN
            CREATE NONCLUSTERED INDEX [IX_Employees_Org_PersonnelCode_Active]
                ON [dbo].[Employees]([OrganizationId], [PersonnelCode])
                WHERE [IsDeleted] = 0;
        END
        """
    ];

    public static async Task EnsureLatestSchemaAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSchemaBootstrap");

        if (!await CanConnectAsync(context, logger))
            return;

        foreach (var patch in SchemaPatches)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(patch);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Schema patch failed — run database/16_Migration_EmployeeRosterSync.sql manually");
            }
        }

        logger.LogInformation("Database schema bootstrap completed (roster sync columns)");
    }

    private static async Task<bool> CanConnectAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            return await context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database not reachable — schema bootstrap skipped");
            return false;
        }
    }
}

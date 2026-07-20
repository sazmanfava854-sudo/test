USE [HRPerformanceDB];
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'LastEmployeeRosterSyncAt') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [LastEmployeeRosterSyncAt] DATETIME2 NULL;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'IsRosterSyncRunning') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [IsRosterSyncRunning] BIT NOT NULL
        CONSTRAINT [DF_AttendanceIntegrationSettings_RosterRunning] DEFAULT 0;
GO

IF COL_LENGTH('dbo.Employees', 'LastSeenInRosterSyncAt') IS NULL
    ALTER TABLE [dbo].[Employees] ADD [LastSeenInRosterSyncAt] DATETIME2 NULL;
GO

IF COL_LENGTH('dbo.AttendanceSyncLogs', 'SyncType') IS NULL
    ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [SyncType] NVARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.AttendanceSyncLogs', 'EmployeesInserted') IS NULL
    ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [EmployeesInserted] INT NOT NULL
        CONSTRAINT [DF_AttendanceSyncLogs_EmpInserted] DEFAULT 0;
GO

IF COL_LENGTH('dbo.AttendanceSyncLogs', 'EmployeesUpdated') IS NULL
    ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [EmployeesUpdated] INT NOT NULL
        CONSTRAINT [DF_AttendanceSyncLogs_EmpUpdated] DEFAULT 0;
GO

IF COL_LENGTH('dbo.AttendanceSyncLogs', 'RequestedByUserName') IS NULL
    ALTER TABLE [dbo].[AttendanceSyncLogs] ADD [RequestedByUserName] NVARCHAR(256) NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Employees_Org_PersonnelCode_Active'
      AND object_id = OBJECT_ID(N'dbo.Employees'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Employees_Org_PersonnelCode_Active]
        ON [dbo].[Employees]([OrganizationId], [PersonnelCode])
        WHERE [IsDeleted] = 0;
END
GO

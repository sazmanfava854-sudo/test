USE [HRPerformanceDB];
GO

-- Allow multiple MIS records per day (hourly leave) keyed by ExternalId
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_AttendanceLogs_EmployeeDate' AND object_id = OBJECT_ID('dbo.AttendanceLogs'))
    ALTER TABLE [dbo].[AttendanceLogs] DROP CONSTRAINT [UQ_AttendanceLogs_EmployeeDate];
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AttendanceLogs_OrgExternalId' AND object_id = OBJECT_ID('dbo.AttendanceLogs'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AttendanceLogs_OrgExternalId]
    ON [dbo].[AttendanceLogs]([OrganizationId], [ExternalId])
    WHERE [ExternalId] IS NOT NULL;
GO

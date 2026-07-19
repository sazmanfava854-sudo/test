USE [HRPerformanceDB];
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'SyncMode') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [SyncMode] NVARCHAR(20) NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_SyncMode] DEFAULT 'Monthly';
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'ShamsiYearPrefix') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [ShamsiYearPrefix] NVARCHAR(4) NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_ShamiYear] DEFAULT '1404';
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'ProvinceCode') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [ProvinceCode] NVARCHAR(20) NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_ProvinceCode] DEFAULT '147';
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'ApplyProvinceFilter') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [ApplyProvinceFilter] BIT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_ApplyProvince] DEFAULT 1;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'ApplyShamsiYearFilter') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [ApplyShamsiYearFilter] BIT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_ApplyShamsi] DEFAULT 1;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'InitialSyncMonthsBack') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [InitialSyncMonthsBack] INT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_InitialMonths] DEFAULT 12;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'MonthsPerSyncRun') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [MonthsPerSyncRun] INT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_MonthsPerRun] DEFAULT 1;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'SyncDaysBack') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [SyncDaysBack] INT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_SyncDaysBack] DEFAULT 30;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'EmployeeLimit') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [EmployeeLimit] INT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_EmployeeLimit] DEFAULT 0;
GO

IF COL_LENGTH('dbo.AttendanceIntegrationSettings', 'BackgroundSyncEnabled') IS NULL
    ALTER TABLE [dbo].[AttendanceIntegrationSettings] ADD [BackgroundSyncEnabled] BIT NOT NULL CONSTRAINT [DF_AttendanceIntegrationSettings_BackgroundSync] DEFAULT 0;
GO

UPDATE [dbo].[AttendanceIntegrationSettings]
SET [BackgroundSyncEnabled] = 0,
    [MonthsPerSyncRun] = CASE WHEN [MonthsPerSyncRun] < 1 THEN 1 ELSE [MonthsPerSyncRun] END;
GO

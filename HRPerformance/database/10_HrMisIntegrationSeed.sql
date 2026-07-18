USE [HRPerformanceDB];
GO

DECLARE @OrgId UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [dbo].[Organizations]);

IF @OrgId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM [dbo].[AttendanceIntegrationSettings] WHERE [OrganizationId] = @OrgId
)
BEGIN
    INSERT INTO [dbo].[AttendanceIntegrationSettings]
    ([Id],[OrganizationId],[SourceType],[SqlViewName],[SyncMode],[ShamsiYearPrefix],[ProvinceCode],
     [ApplyProvinceFilter],[ApplyShamsiYearFilter],[InitialSyncMonthsBack],[MonthsPerSyncRun],
     [SyncDaysBack],[EmployeeLimit],[BackgroundSyncEnabled],[SyncIntervalMinutes],[IsActive],[CreatedAt])
    VALUES
    (NEWID(), @OrgId, 'SQLView', 'MIS.dbo.HZG_View_HourlyLeave', 'Monthly', '1404', '147',
     1, 1, 12, 1, 30, 0, 0, 5, 1, SYSUTCDATETIME());
END
GO

-- Sample scoring rules for hourly leave duration (optional - adjust in admin panel)
DECLARE @OrgId2 UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [dbo].[Organizations]);
IF @OrgId2 IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM [dbo].[EvaluationRules] WHERE [OrganizationId] = @OrgId2 AND [ConditionType] = 5
)
BEGIN
    INSERT INTO [dbo].[EvaluationRules]
    ([Id],[OrganizationId],[Name],[Description],[ConditionType],[Operator],[MinValue],[MaxValue],[ScoreImpact],[Priority],[IsActive],[CreatedAt],[IsDeleted])
    VALUES
    (NEWID(), @OrgId2, N'مرخصی ساعتی تا ۳۰ دقیقه', N'بدون امتیاز منفی', 5, 4, 0, 30, 0, 1, 1, SYSUTCDATETIME(), 0),
    (NEWID(), @OrgId2, N'مرخصی ساعتی ۳۱ تا ۶۰ دقیقه', N'امتیاز منفی خفیف', 5, 6, 31, 60, -1, 2, 1, SYSUTCDATETIME(), 0),
    (NEWID(), @OrgId2, N'مرخصی ساعتی بالای ۶۰ دقیقه', N'امتیاز منفی', 5, 5, 60, NULL, -3, 3, 1, SYSUTCDATETIME(), 0);
END
GO

USE [HRPerformanceDB];
GO

-- شاخص‌های ارزیابی (رادار داشبورد مدیر)
DECLARE @OrgId UNIQUEIDENTIFIER;
SELECT TOP 1 @OrgId = [Id] FROM [dbo].[Organizations] ORDER BY [CreatedAt];

IF @OrgId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[EvaluationCategories] WHERE [OrganizationId] = @OrgId)
BEGIN
    INSERT INTO [dbo].[EvaluationCategories]([Id],[OrganizationId],[Name],[Description],[Color],[Icon],[Weight],[IsActive])
    VALUES
    (NEWID(), @OrgId, N'کیفیت کار', N'کیفیت انجام وظایف', '#4CAF50', N'star', 25, 1),
    (NEWID(), @OrgId, N'حضور و غیاب', N'حضور به‌موقع و مشارکت', '#2196F3', N'event', 25, 1),
    (NEWID(), @OrgId, N'انضباط', N'رعایت مقررات سازمانی', '#FF9800', N'gavel', 20, 1),
    (NEWID(), @OrgId, N'کار تیمی', N'همکاری با همکاران', '#9C27B0', N'groups', 15, 1),
    (NEWID(), @OrgId, N'بهره‌وری', N'میزان خروجی و اثربخشی', '#F44336', N'trending_up', 15, 1);
END
GO

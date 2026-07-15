USE [HRPerformanceDB];
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_CalculateEmployeeRankings] @OrganizationId UNIQUEIDENTIFIER,@Year INT,@Month INT=NULL AS
BEGIN SET NOCOUNT ON;
DELETE FROM [dbo].[Rankings] WHERE [OrganizationId]=@OrganizationId AND [EntityType]='Employee' AND [Year]=@Year;
;WITH S AS(SELECT e.[Id],ISNULL(SUM(es.[Score]),0) AS T FROM [dbo].[Employees] e
LEFT JOIN [dbo].[EmployeeScores] es ON e.[Id]=es.[EmployeeId] AND es.[Year]=@Year AND (@Month IS NULL OR es.[Month]=@Month)
WHERE e.[OrganizationId]=@OrganizationId AND e.[IsDeleted]=0 GROUP BY e.[Id]),
R AS(SELECT [Id],T,RANK() OVER(ORDER BY T DESC) AS Rk FROM S)
INSERT INTO [dbo].[Rankings]([OrganizationId],[EntityType],[EntityId],[Rank],[Score],[Year],[Month],[PeriodType])
SELECT @OrganizationId,'Employee',[Id],[Rk],T,@Year,@Month,1 FROM R;
UPDATE e SET e.[Ranking]=r.[Rank] FROM [dbo].[Employees] e INNER JOIN [dbo].[Rankings] r ON e.[Id]=r.[EntityId]
WHERE r.[OrganizationId]=@OrganizationId AND r.[Year]=@Year;
END
GO

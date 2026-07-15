USE [HRPerformanceDB];
GO
CREATE OR ALTER VIEW [dbo].[vw_EmployeeSummary] AS
SELECT e.[Id],e.[OrganizationId],e.[PersonnelCode],e.[FirstName],e.[LastName],
e.[FirstName]+N' '+e.[LastName] AS [FullName],e.[Position],e.[Status],e.[CurrentScore],
e.[MonthlyScore],e.[YearlyScore],e.[Ranking],ou.[Name] AS [DepartmentName]
FROM [dbo].[Employees] e LEFT JOIN [dbo].[OrganizationUnits] ou ON e.[OrganizationUnitId]=ou.[Id] WHERE e.[IsDeleted]=0;
GO

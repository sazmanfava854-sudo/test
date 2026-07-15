USE [HRPerformanceDB];
GO
CREATE OR ALTER TRIGGER [dbo].[tr_EmployeeScores_AfterInsert] ON [dbo].[EmployeeScores] AFTER INSERT AS
BEGIN SET NOCOUNT ON;
UPDATE e SET e.[MonthlyScore]=ISNULL((SELECT SUM(es.[Score]) FROM [dbo].[EmployeeScores] es WHERE es.[EmployeeId]=e.[Id] AND es.[Year]=i.[Year] AND es.[Month]=i.[Month]),0),
e.[YearlyScore]=ISNULL((SELECT SUM(es.[Score]) FROM [dbo].[EmployeeScores] es WHERE es.[EmployeeId]=e.[Id] AND es.[Year]=i.[Year]),0),
e.[CurrentScore]=ISNULL((SELECT SUM(es.[Score]) FROM [dbo].[EmployeeScores] es WHERE es.[EmployeeId]=e.[Id] AND es.[Year]=i.[Year] AND es.[Month]=i.[Month]),0)
FROM [dbo].[Employees] e INNER JOIN inserted i ON e.[Id]=i.[EmployeeId];
END
GO

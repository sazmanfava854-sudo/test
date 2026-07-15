USE [HRPerformanceDB];
GO
CREATE NONCLUSTERED INDEX [IX_Employees_Org] ON [dbo].[Employees]([OrganizationId]) WHERE [IsDeleted]=0;
CREATE NONCLUSTERED INDEX [IX_Employees_Mgr] ON [dbo].[Employees]([ManagerId]) WHERE [IsDeleted]=0;
CREATE NONCLUSTERED INDEX [IX_Attendance_EmpDate] ON [dbo].[AttendanceLogs]([EmployeeId],[AttendanceDate] DESC);
CREATE NONCLUSTERED INDEX [IX_Scores_Emp] ON [dbo].[EmployeeScores]([EmployeeId],[ScoreDate] DESC);
CREATE NONCLUSTERED INDEX [IX_Notif_User] ON [dbo].[Notifications]([UserId],[IsRead],[CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_Audit_Date] ON [dbo].[AuditLogs]([CreatedAt] DESC);
GO

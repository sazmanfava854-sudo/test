USE [HRPerformanceDB];
GO
ALTER TABLE [dbo].[OrganizationUnits] ADD CONSTRAINT [FK_OU_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
ALTER TABLE [dbo].[OrganizationUnits] ADD CONSTRAINT [FK_OU_Parent] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[OrganizationUnits]([Id]);
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
ALTER TABLE [dbo].[RolePermissions] ADD CONSTRAINT [FK_RP_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[RolePermissions] ADD CONSTRAINT [FK_RP_Perm] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [FK_UR_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [FK_UR_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[RefreshTokens] ADD CONSTRAINT [FK_RT_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Employees] ADD CONSTRAINT [FK_Emp_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
ALTER TABLE [dbo].[Employees] ADD CONSTRAINT [FK_Emp_OU] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [dbo].[OrganizationUnits]([Id]);
ALTER TABLE [dbo].[Employees] ADD CONSTRAINT [FK_Emp_Mgr] FOREIGN KEY ([ManagerId]) REFERENCES [dbo].[Employees]([Id]);
ALTER TABLE [dbo].[EvaluationCategories] ADD CONSTRAINT [FK_EC_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
ALTER TABLE [dbo].[EvaluationItems] ADD CONSTRAINT [FK_EI_Cat] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[EvaluationCategories]([Id]);
ALTER TABLE [dbo].[EvaluationRules] ADD CONSTRAINT [FK_ER_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
ALTER TABLE [dbo].[AttendanceLogs] ADD CONSTRAINT [FK_AL_Emp] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]);
ALTER TABLE [dbo].[EmployeeScores] ADD CONSTRAINT [FK_ES_Emp] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]);
ALTER TABLE [dbo].[EmployeeEvaluations] ADD CONSTRAINT [FK_EE_Emp] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]);
ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_N_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Appeals] ADD CONSTRAINT [FK_A_Emp] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]);
ALTER TABLE [dbo].[Settings] ADD CONSTRAINT [FK_S_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
ALTER TABLE [dbo].[Holidays] ADD CONSTRAINT [FK_H_Org] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]);
GO

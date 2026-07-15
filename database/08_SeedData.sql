USE [HRPerformanceDB];
GO
DECLARE @OrgId UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleSuperAdmin UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleOrgAdmin UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleManager UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleEmployee UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
DECLARE @DeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @EmpId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Organizations]([Id],[Name],[Code],[IsActive]) VALUES(@OrgId,N'شهرداری مشهد','MSH001',1);

INSERT INTO [dbo].[Roles]([Id],[Name],[NormalizedName],[Description]) VALUES
(@RoleSuperAdmin,'SuperAdministrator','SUPERADMINISTRATOR',N'مدیر کل سیستم'),
(@RoleOrgAdmin,'OrganizationAdministrator','ORGANIZATIONADMINISTRATOR',N'مدیر سازمان'),
(@RoleManager,'Manager','MANAGER',N'مدیر'),
(@RoleEmployee,'Employee','EMPLOYEE',N'کارمند');

INSERT INTO [dbo].[Permissions]([Id],[Name],[Code],[Module]) VALUES
(NEWID(),N'مدیریت کاربران','users.manage','Users'),
(NEWID(),N'مدیریت کارمندان','employees.manage','Employees'),
(NEWID(),N'مشاهده داشبورد','dashboard.view','Dashboard'),
(NEWID(),N'مدیریت ارزیابی','evaluations.manage','Evaluations'),
(NEWID(),N'مشاهده گزارشات','reports.view','Reports');

INSERT INTO [dbo].[Users]([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],
[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[FirstName],[LastName],[OrganizationId],[IsActive])
VALUES(@UserId,'admin','ADMIN','admin@hr.local','ADMIN@HR.LOCAL',1,
'AQAAAAIAAYagAAAAEPlaceholderHashReplaceOnFirstLogin','SEC','CONC',N'مدیر',N'سیستم',@OrgId,1);

INSERT INTO [dbo].[UserRoles]([UserId],[RoleId]) VALUES(@UserId,@RoleSuperAdmin);

INSERT INTO [dbo].[OrganizationUnits]([Id],[OrganizationId],[Name],[Code],[UnitType],[Level],[IsActive])
VALUES(@DeptId,@OrgId,N'اداره منابع انسانی','HR-001',2,0,1);

INSERT INTO [dbo].[Employees]([Id],[OrganizationId],[OrganizationUnitId],[PersonnelCode],[NationalCode],
[FirstName],[LastName],[EmploymentDate],[EmploymentType],[Position],[Status])
VALUES(@EmpId,@OrgId,@DeptId,'10001','0012345678',N'مدیر',N'سیستم',GETDATE(),1,N'مدیر سیستم',1);

UPDATE [dbo].[Users] SET [EmployeeId]=@EmpId WHERE [Id]=@UserId;

INSERT INTO [dbo].[Settings]([Id],[OrganizationId],[Key],[Value],[Category],[DataType]) VALUES
(NEWID(),@OrgId,'ScoreThreshold','60','Scoring','decimal'),
(NEWID(),@OrgId,'WorkingHoursStart','07:30','Attendance','time'),
(NEWID(),@OrgId,'WorkingHoursEnd','15:00','Attendance','time'),
(NEWID(),@OrgId,'DelayToleranceMinutes','10','Attendance','int');
GO

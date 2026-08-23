-- گروه‌های کاربری و دسترسی فرم‌ها
IF OBJECT_ID(N'dbo.AppUserGroup', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUserGroup (
        Id                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AppUserGroup PRIMARY KEY,
        Name                    NVARCHAR(100)    NOT NULL,
        CanAccessUnsentFiches   BIT              NOT NULL CONSTRAINT DF_AppUserGroup_Unsent DEFAULT (0),
        CanAccessInstallment    BIT              NOT NULL CONSTRAINT DF_AppUserGroup_Installment DEFAULT (0),
        CanManageUsers          BIT              NOT NULL CONSTRAINT DF_AppUserGroup_Users DEFAULT (0),
        CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_AppUserGroup_Created DEFAULT (SYSUTCDATETIME())
    );
    CREATE UNIQUE INDEX UQ_AppUserGroup_Name ON dbo.AppUserGroup (Name);
END

IF OBJECT_ID(N'dbo.AppUserGroupMember', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUserGroupMember (
        UserId  UNIQUEIDENTIFIER NOT NULL,
        GroupId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_AppUserGroupMember PRIMARY KEY (UserId, GroupId),
        CONSTRAINT FK_AppUserGroupMember_User FOREIGN KEY (UserId) REFERENCES dbo.AppUser (Id),
        CONSTRAINT FK_AppUserGroupMember_Group FOREIGN KEY (GroupId) REFERENCES dbo.AppUserGroup (Id)
    );
END

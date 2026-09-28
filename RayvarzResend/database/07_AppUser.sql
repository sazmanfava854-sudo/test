-- App users for Rayvarz Resend login (auto-created by AppUserRepository if missing)
IF OBJECT_ID(N'dbo.AppUser', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUser (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AppUser PRIMARY KEY,
        Username        NVARCHAR(100)    NOT NULL,
        PasswordHash    NVARCHAR(500)    NOT NULL,
        FirstName       NVARCHAR(100)    NOT NULL CONSTRAINT DF_AppUser_FirstName DEFAULT (N''),
        LastName        NVARCHAR(100)    NOT NULL CONSTRAINT DF_AppUser_LastName DEFAULT (N''),
        NationalId      NVARCHAR(20)     NOT NULL CONSTRAINT DF_AppUser_NationalId DEFAULT (N''),
        Position        NVARCHAR(200)    NOT NULL CONSTRAINT DF_AppUser_Position DEFAULT (N''),
        District        NVARCHAR(50)     NOT NULL CONSTRAINT DF_AppUser_District DEFAULT (N''),
        [Domain]        NVARCHAR(100)    NOT NULL CONSTRAINT DF_AppUser_Domain DEFAULT (N''),
        IsAdmin         BIT              NOT NULL CONSTRAINT DF_AppUser_IsAdmin DEFAULT (0),
        IsActive        BIT              NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT (1),
        CreatedAtUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_AppUser_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_AppUser_Username UNIQUE (Username)
    );
    CREATE INDEX IX_AppUser_Active ON dbo.AppUser (IsActive) INCLUDE (Username, IsAdmin);
END

IF COL_LENGTH(N'dbo.AppUser', N'Domain') IS NULL
    ALTER TABLE dbo.AppUser ADD [Domain] NVARCHAR(100) NOT NULL
        CONSTRAINT DF_AppUser_Domain DEFAULT (N'');

-- دامین‌های اضافه برای یک کاربر (مثلاً دو حساب ویندوز برای یک نفر)
IF OBJECT_ID(N'dbo.AppUserDomainAlias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUserDomainAlias (
        [Domain]     NVARCHAR(100)    NOT NULL CONSTRAINT PK_AppUserDomainAlias PRIMARY KEY,
        UserId       UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc DATETIME2(3)     NOT NULL CONSTRAINT DF_AppUserDomainAlias_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AppUserDomainAlias_User FOREIGN KEY (UserId) REFERENCES dbo.AppUser (Id)
    );
    CREATE INDEX IX_AppUserDomainAlias_User ON dbo.AppUserDomainAlias (UserId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UQ_AppUser_Domain' AND object_id = OBJECT_ID(N'dbo.AppUser'))
    CREATE UNIQUE INDEX UQ_AppUser_Domain ON dbo.AppUser ([Domain])
        WHERE [Domain] <> N'';

-- دامین لاگین یکپارچه برای کاربر 0925569917 (کد ملی / نام کاربری)
UPDATE dbo.AppUser
SET [Domain] = N'0925569917'
WHERE (NationalId = N'0925569917' OR Username = N'0925569917')
  AND ISNULL([Domain], N'') = N''
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.AppUser x
      WHERE x.[Domain] = N'0925569917'
        AND x.NationalId <> N'0925569917'
        AND x.Username <> N'0925569917');

-- دامین دوم برای کاربر 0925569917 (hoseine-sh + sadathoseini-sh)
INSERT INTO dbo.AppUserDomainAlias ([Domain], UserId)
SELECT TOP 1 N'sadathoseini-sh', u.Id
FROM dbo.AppUser u
WHERE (u.NationalId = N'0925569917' OR u.Username = N'0925569917')
  AND NOT EXISTS (SELECT 1 FROM dbo.AppUserDomainAlias a WHERE a.[Domain] = N'sadathoseini-sh')
  AND NOT EXISTS (SELECT 1 FROM dbo.AppUser x WHERE x.[Domain] = N'sadathoseini-sh' AND x.Id <> u.Id)
ORDER BY CASE WHEN u.Username = N'0925569917' THEN 0 ELSE 1 END;

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

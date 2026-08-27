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
        IsAdmin         BIT              NOT NULL CONSTRAINT DF_AppUser_IsAdmin DEFAULT (0),
        IsActive        BIT              NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT (1),
        CreatedAtUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_AppUser_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_AppUser_Username UNIQUE (Username)
    );
    CREATE INDEX IX_AppUser_Active ON dbo.AppUser (IsActive) INCLUDE (Username, IsAdmin);
END

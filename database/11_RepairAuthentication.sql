USE [HRPerformanceDB];
GO

-- بازنشانی رمز admin — همیشه Admin@123
-- Hash معتبر ASP.NET Core Identity برای Admin@123
UPDATE [dbo].[Users]
SET [PasswordHash] =
        'AQAAAAIAAYagAAAAEE6v9aYxxq5Mwu8wmjbwKyVue/lYmYNZ2Dte3bzJG6nNReQWP/s55XxgmbiSsqTvKw==',
    [SecurityStamp] = CONVERT(NVARCHAR(36), NEWID()),
    [ConcurrencyStamp] = CONVERT(NVARCHAR(36), NEWID()),
    [AccessFailedCount] = 0,
    [LockoutEnd] = NULL,
    [IsActive] = 1
WHERE [NormalizedUserName] = 'ADMIN';
GO

IF @@ROWCOUNT = 0
BEGIN
    PRINT N'کاربر admin یافت نشد — ابتدا database/08_SeedData.sql را اجرا کنید.';
END
ELSE
BEGIN
    PRINT N'رمز admin به Admin@123 بازنشانی شد.';
END
GO

-- جدول RefreshTokens در دیتابیس‌های قدیمی
IF OBJECT_ID(N'[dbo].[RefreshTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id]              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [UserId]          UNIQUEIDENTIFIER NOT NULL,
        [Token]           NVARCHAR(500) NOT NULL,
        [ExpiresAt]       DATETIME2 NOT NULL,
        [CreatedAt]       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [CreatedByIp]     NVARCHAR(50) NULL,
        [RevokedAt]       DATETIME2 NULL,
        [RevokedByIp]     NVARCHAR(50) NULL,
        [ReplacedByToken] NVARCHAR(500) NULL,
        [IsRevoked]       BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_RT_User] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_RefreshTokens_Token]
        ON [dbo].[RefreshTokens]([Token]);
END
GO

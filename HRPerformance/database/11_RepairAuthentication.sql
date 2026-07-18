USE [HRPerformanceDB];
GO

-- Repair the invalid placeholder hash shipped in older local packages.
-- Password after repair: Admin@123
UPDATE [dbo].[Users]
SET [PasswordHash] =
        'AQAAAAIAAYagAAAAEE6v9aYxxq5Mwu8wmjbwKyVue/lYmYNZ2Dte3bzJG6nNReQWP/s55XxgmbiSsqTvKw==',
    [SecurityStamp] = CONVERT(NVARCHAR(36), NEWID()),
    [ConcurrencyStamp] = CONVERT(NVARCHAR(36), NEWID()),
    [AccessFailedCount] = 0,
    [LockoutEnd] = NULL
WHERE [NormalizedUserName] = 'ADMIN'
  AND [PasswordHash] = 'AQAAAAIAAYagAAAAEPlaceholderHashReplaceOnFirstLogin';
GO

-- Older/incomplete databases may not contain the refresh-token table.
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

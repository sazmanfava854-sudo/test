USE [HRPerformanceDB];
GO

IF OBJECT_ID(N'[dbo].[HrMisSyncStates]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrMisSyncStates] (
        [OrganizationId]        UNIQUEIDENTIFIER NOT NULL,
        [TargetShamsiYear]      INT              NOT NULL,
        [NextShamsiMonth]       INT              NOT NULL,
        [BackfillStartMonth]    INT              NOT NULL DEFAULT 1,
        [IsBackfillComplete]    BIT              NOT NULL DEFAULT 0,
        [LastSyncedAt]          DATETIME2        NULL,
        [LastSyncDescription]   NVARCHAR(200)    NULL,
        [CreatedAt]             DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt]             DATETIME2        NULL,
        CONSTRAINT [PK_HrMisSyncStates] PRIMARY KEY CLUSTERED ([OrganizationId])
    );
END
GO

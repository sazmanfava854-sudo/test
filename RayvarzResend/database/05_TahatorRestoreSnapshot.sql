-- Tahator — persist Income_Fiche fields before status=2 so restore survives process crash.
-- Run on RayvarzRuleEngine (same DB as rule-engine state). Does NOT modify Sara8M03.

USE RayvarzRuleEngine;
GO

IF OBJECT_ID(N'dbo.TahatorRestoreSnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TahatorRestoreSnapshot (
        SnapshotId              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TahatorRestoreSnapshot PRIMARY KEY,
        FicheNo                 NVARCHAR(50)  NOT NULL,
        EumFicheStatus          INT           NOT NULL,
        ExportPermanentDate     NVARCHAR(30)  NULL,
        PaymentBreakDate        NVARCHAR(30)  NULL,
        PaymentDate             NVARCHAR(30)  NULL,
        UserConfirmDate         NVARCHAR(30)  NULL,
        UsernameUserConfirm     NVARCHAR(200) NULL,
        NidUserUserConfirm      UNIQUEIDENTIFIER NULL,
        TriggerDate             NVARCHAR(30)  NULL,
        Status                  VARCHAR(30)   NOT NULL CONSTRAINT DF_TahatorRestore_Status DEFAULT ('Pending'),
        CreatedAtUtc            DATETIME2(3)  NOT NULL CONSTRAINT DF_TahatorRestore_Created DEFAULT (SYSUTCDATETIME()),
        RestoredAtUtc           DATETIME2(3)  NULL,
        Notes                   NVARCHAR(500) NULL
    );

    CREATE INDEX IX_TahatorRestore_Fiche_Status
        ON dbo.TahatorRestoreSnapshot (FicheNo, Status)
        INCLUDE (CreatedAtUtc);
END
GO

PRINT 'TahatorRestoreSnapshot ready.';
GO

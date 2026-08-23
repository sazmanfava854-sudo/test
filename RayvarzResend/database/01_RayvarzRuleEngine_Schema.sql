-- RayvarzResend Phase 0 — Rule Engine state database
-- Run on server 232 (or your SQL host). Does NOT modify Sara8M03 or DbRuleEngein.

IF DB_ID(N'RayvarzRuleEngine') IS NULL
    CREATE DATABASE RayvarzRuleEngine;
GO

USE RayvarzRuleEngine;
GO

IF OBJECT_ID(N'dbo.RuleSyncState', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleSyncState (
        NidMember           INT           NOT NULL CONSTRAINT PK_RuleSyncState PRIMARY KEY,
        NidClass            INT           NOT NULL CONSTRAINT DF_RuleSyncState_NidClass DEFAULT (360),
        LastSeenNidHistory  BIGINT        NULL,
        LastSeenModifyAt    DATETIME2(3)  NULL,
        LastStableNidHistory BIGINT       NULL,
        LastStableModifyAt  DATETIME2(3)  NULL,
        LastStableXmlHash   CHAR(64)      NULL,
        ActiveDslVersion    INT           NOT NULL CONSTRAINT DF_RuleSyncState_ActiveDsl DEFAULT (0),
        ActiveEngine        VARCHAR(20)   NOT NULL CONSTRAINT DF_RuleSyncState_Engine DEFAULT ('Legacy'),
        ActiveSnapshotId    BIGINT        NULL,
        UpdatedAtUtc        DATETIME2(3)  NOT NULL CONSTRAINT DF_RuleSyncState_Updated DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.RuleCandidate', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleCandidate (
        CandidateId           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleCandidate PRIMARY KEY,
        NidMember             INT           NOT NULL,
        SourceNidHistory      BIGINT        NOT NULL,
        SourceModifyAt        DATETIME2(3)  NOT NULL,
        CanonicalXmlHash      CHAR(64)      NOT NULL,
        XmlBody               NVARCHAR(MAX) NOT NULL,
        Modifyer              NVARCHAR(200) NULL,
        ModifyDesc            NVARCHAR(500) NULL,
        Status                VARCHAR(30)   NOT NULL,
        RejectReason          NVARCHAR(500) NULL,
        StableEligibleAtUtc   DATETIME2(3)  NOT NULL,
        FirstSeenAtUtc        DATETIME2(3)  NOT NULL CONSTRAINT DF_RuleCandidate_FirstSeen DEFAULT (SYSUTCDATETIME()),
        LastCheckedAtUtc      DATETIME2(3)  NOT NULL CONSTRAINT DF_RuleCandidate_LastChecked DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_RuleCandidate_Hash UNIQUE (NidMember, CanonicalXmlHash)
    );
    CREATE INDEX IX_RuleCandidate_Member_Status ON dbo.RuleCandidate (NidMember, Status, SourceModifyAt DESC);
END
GO

IF OBJECT_ID(N'dbo.RuleDslSnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleDslSnapshot (
        SnapshotId      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleDslSnapshot PRIMARY KEY,
        NidMember       INT           NOT NULL,
        DslVersion      INT           NOT NULL,
        XmlHash         CHAR(64)      NOT NULL,
        DslJson         NVARCHAR(MAX) NULL,
        ParserVersion   VARCHAR(20)   NOT NULL CONSTRAINT DF_RuleDslSnapshot_Parser DEFAULT ('0.0.0'),
        EntryPoint      VARCHAR(50)   NOT NULL CONSTRAINT DF_RuleDslSnapshot_Entry DEFAULT ('Run'),
        CreatedAtUtc    DATETIME2(3)  NOT NULL CONSTRAINT DF_RuleDslSnapshot_Created DEFAULT (SYSUTCDATETIME()),
        IsActive        BIT           NOT NULL CONSTRAINT DF_RuleDslSnapshot_Active DEFAULT (0)
    );
    CREATE INDEX IX_RuleDslSnapshot_Member ON dbo.RuleDslSnapshot (NidMember, IsActive, DslVersion DESC);
END
GO

IF OBJECT_ID(N'dbo.RulePromotionLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RulePromotionLog (
        LogId         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RulePromotionLog PRIMARY KEY,
        NidMember     INT           NOT NULL,
        CandidateId   BIGINT        NULL,
        SnapshotId    BIGINT        NULL,
        Action        VARCHAR(30)   NOT NULL,
        Reason        NVARCHAR(500) NULL,
        CreatedAtUtc  DATETIME2(3)  NOT NULL CONSTRAINT DF_RulePromotionLog_Created DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.RuleDryRunResult', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleDryRunResult (
        DryRunId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleDryRunResult PRIMARY KEY,
        CandidateId     BIGINT        NULL,
        SnapshotId      BIGINT        NULL,
        GoldenFicheId   INT           NOT NULL,
        EngineName      VARCHAR(20)   NOT NULL,
        Success         BIT           NOT NULL,
        ErrorMessage    NVARCHAR(MAX) NULL,
        OutputJson      NVARCHAR(MAX) NULL,
        ExecutedAtUtc   DATETIME2(3)  NOT NULL CONSTRAINT DF_RuleDryRunResult_Executed DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_RuleDryRunResult_Golden ON dbo.RuleDryRunResult (GoldenFicheId, ExecutedAtUtc DESC);
END
GO

IF OBJECT_ID(N'dbo.RuleGoldenFiche', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleGoldenFiche (
        GoldenFicheId     INT           NOT NULL CONSTRAINT PK_RuleGoldenFiche PRIMARY KEY,
        Name              NVARCHAR(100) NOT NULL,
        FicheNo           NVARCHAR(50)  NOT NULL,
        NidFiche          UNIQUEIDENTIFIER NOT NULL,
        NidMember         INT           NOT NULL CONSTRAINT DF_RuleGoldenFiche_Member DEFAULT (1388),
        Scenario          NVARCHAR(50)  NOT NULL,
        ExpectedRowCount  INT           NOT NULL,
        IsActive          BIT           NOT NULL CONSTRAINT DF_RuleGoldenFiche_Active DEFAULT (1),
        Notes             NVARCHAR(500) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.RuleGoldenExpectedRow', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleGoldenExpectedRow (
        GoldenFicheId   INT           NOT NULL,
        IncmRow         INT           NOT NULL,
        IncmNo          INT           NOT NULL,
        ExpectedVal     DECIMAL(18,2) NOT NULL,
        IncmRowDsc      NVARCHAR(200) NULL,
        ExpectedBranch  INT           NULL,
        ExpectedBank    INT           NULL,
        CONSTRAINT PK_RuleGoldenExpectedRow PRIMARY KEY (GoldenFicheId, IncmRow),
        CONSTRAINT FK_RuleGoldenExpectedRow_Fiche FOREIGN KEY (GoldenFicheId)
            REFERENCES dbo.RuleGoldenFiche (GoldenFicheId)
    );
END
GO

-- Initial sync row for NidMember 1388
IF NOT EXISTS (SELECT 1 FROM dbo.RuleSyncState WHERE NidMember = 1388)
    INSERT INTO dbo.RuleSyncState (NidMember, NidClass, ActiveEngine, ActiveDslVersion)
    VALUES (1388, 360, 'Legacy', 0);
GO

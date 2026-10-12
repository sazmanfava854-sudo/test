-- Rule Engine sync / version state (RayvarzRuleEngine)
-- Member + MemberHistory remain on DbRuleEngein (ConnectionStrings:RuleEngine).

USE RayvarzRuleEngine;
GO

IF OBJECT_ID(N'dbo.RuleSyncState', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleSyncState (
        NidMember               INT              NOT NULL CONSTRAINT PK_RuleSyncState PRIMARY KEY,
        ActiveEngine            NVARCHAR(32)     NOT NULL CONSTRAINT DF_RuleSyncState_Engine DEFAULT (N'Legacy'),
        ActiveXmlBodyHash       NVARCHAR(64)     NOT NULL CONSTRAINT DF_RuleSyncState_Hash DEFAULT (N''),
        ActiveMemberVersion     INT              NOT NULL CONSTRAINT DF_RuleSyncState_Version DEFAULT (0),
        LastSeenNidHistory      BIGINT           NULL,
        LastHistoryModifyUtc    DATETIME2(3)     NULL,
        LastMemberXmlBodyHash   NVARCHAR(64)     NOT NULL CONSTRAINT DF_RuleSyncState_MemberHash DEFAULT (N''),
        LastCheckAtUtc          DATETIME2(3)     NULL,
        LastChangeDetectedAtUtc DATETIME2(3)     NULL,
        LastPromotedAtUtc       DATETIME2(3)     NULL,
        LastError               NVARCHAR(2000)   NULL,
        UpdatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_RuleSyncState_Updated DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.RuleCandidate', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleCandidate (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RuleCandidate PRIMARY KEY,
        NidMember           INT              NOT NULL,
        NidHistory          BIGINT           NULL,
        XmlBodyHash         NVARCHAR(64)     NOT NULL,
        MemberVersion       INT              NOT NULL CONSTRAINT DF_RuleCandidate_Version DEFAULT (0),
        State               NVARCHAR(32)     NOT NULL,
        DetectedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_RuleCandidate_Detected DEFAULT (SYSUTCDATETIME()),
        StableSinceUtc      DATETIME2(3)     NULL,
        ParseOk             BIT              NOT NULL CONSTRAINT DF_RuleCandidate_Parse DEFAULT (0),
        ValidationOk        BIT              NOT NULL CONSTRAINT DF_RuleCandidate_Val DEFAULT (0),
        DryRunOk            BIT              NOT NULL CONSTRAINT DF_RuleCandidate_Dry DEFAULT (0),
        Notes               NVARCHAR(2000)   NULL,
        UpdatedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_RuleCandidate_Updated DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_RuleCandidate_Member_State ON dbo.RuleCandidate (NidMember, State, DetectedAtUtc DESC);
END
GO

IF OBJECT_ID(N'dbo.RuleDslSnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleDslSnapshot (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RuleDslSnapshot PRIMARY KEY,
        NidMember           INT              NOT NULL,
        XmlBodyHash         NVARCHAR(64)     NOT NULL,
        NidHistory          BIGINT           NULL,
        SnapshotJson        NVARCHAR(MAX)    NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_RuleDslSnapshot_Created DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_RuleDslSnapshot_Hash ON dbo.RuleDslSnapshot (NidMember, XmlBodyHash);
END
GO

IF OBJECT_ID(N'dbo.RulePromotionLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RulePromotionLog (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RulePromotionLog PRIMARY KEY,
        NidMember           INT              NOT NULL,
        CandidateId         UNIQUEIDENTIFIER NULL,
        FromEngine          NVARCHAR(32)     NOT NULL,
        ToEngine            NVARCHAR(32)     NOT NULL,
        XmlBodyHash         NVARCHAR(64)     NOT NULL,
        NidHistory          BIGINT           NULL,
        Outcome             NVARCHAR(32)     NOT NULL,
        Detail              NVARCHAR(2000)   NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_RulePromotionLog_Created DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_RulePromotionLog_Member ON dbo.RulePromotionLog (NidMember, CreatedAtUtc DESC);
END
GO

IF OBJECT_ID(N'dbo.RuleDryRunResult', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleDryRunResult (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RuleDryRunResult PRIMARY KEY,
        CandidateId         UNIQUEIDENTIFIER NULL,
        NidMember           INT              NOT NULL,
        GoldenFicheId       INT              NULL,
        Passed              BIT              NOT NULL,
        Summary             NVARCHAR(2000)   NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_RuleDryRunResult_Created DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_RuleDryRunResult_Candidate ON dbo.RuleDryRunResult (CandidateId, CreatedAtUtc DESC);
END
GO

IF OBJECT_ID(N'dbo.RuleGoldenFiche', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleGoldenFiche (
        GoldenFicheId       INT              NOT NULL CONSTRAINT PK_RuleGoldenFiche PRIMARY KEY,
        Name                NVARCHAR(200)    NOT NULL,
        FicheNo             NVARCHAR(32)     NOT NULL,
        NidFiche            UNIQUEIDENTIFIER NULL,
        NidMember           INT              NOT NULL,
        Scenario            NVARCHAR(64)     NOT NULL CONSTRAINT DF_RuleGolden_Scenario DEFAULT (N'Income'),
        ExpectedRowCount    INT              NOT NULL CONSTRAINT DF_RuleGolden_Rows DEFAULT (1),
        Notes               NVARCHAR(500)    NULL,
        IsActive            BIT              NOT NULL CONSTRAINT DF_RuleGolden_Active DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.RuleGoldenExpectedRow', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleGoldenExpectedRow (
        GoldenFicheId       INT              NOT NULL,
        IncmRow             INT              NOT NULL,
        IncmNo              INT              NOT NULL,
        ExpectedVal         DECIMAL(18, 2)   NOT NULL,
        IncmRowDsc          NVARCHAR(500)    NULL,
        ExpectedBranch      INT              NULL,
        ExpectedBank        INT              NULL,
        ExpectedCenter      BIGINT           NULL,
        ExpectedCenter1     BIGINT           NULL,
        ExpectedCenter2     BIGINT           NULL,
        ExpectedCenter3     BIGINT           NULL,
        CONSTRAINT PK_RuleGoldenExpectedRow PRIMARY KEY (GoldenFicheId, IncmRow)
    );
END
GO

PRINT N'Rule Engine sync schema ready.';
GO

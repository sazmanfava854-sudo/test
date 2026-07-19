-- =====================================================
-- HR Performance & Discipline Management System
-- Tables Script
-- =====================================================

USE [HRPerformanceDB];
GO

-- Organizations
CREATE TABLE [dbo].[Organizations] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(200)    NOT NULL,
    [Code]              NVARCHAR(50)     NOT NULL,
    [LogoPath]          NVARCHAR(500)    NULL,
    [Address]           NVARCHAR(500)    NULL,
    [Phone]             NVARCHAR(20)     NULL,
    [Email]             NVARCHAR(200)    NULL,
    [Website]           NVARCHAR(200)    NULL,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Organizations] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Organizations_Code] UNIQUE ([Code])
);
GO

-- Organization Units (Dynamic Hierarchy)
CREATE TABLE [dbo].[OrganizationUnits] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [ParentId]          UNIQUEIDENTIFIER NULL,
    [Name]              NVARCHAR(200)    NOT NULL,
    [Code]              NVARCHAR(50)     NOT NULL,
    [UnitType]          INT              NOT NULL, -- 1=Deputy, 2=Department, 3=Unit, 4=Section
    [Level]             INT              NOT NULL DEFAULT 0,
    [Path]              NVARCHAR(1000)   NULL,
    [ManagerId]         UNIQUEIDENTIFIER NULL,
    [SortOrder]         INT              NOT NULL DEFAULT 0,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_OrganizationUnits] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- ASP.NET Identity Users
CREATE TABLE [dbo].[Users] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserName]          NVARCHAR(256)    NOT NULL,
    [NormalizedUserName] NVARCHAR(256)   NOT NULL,
    [Email]             NVARCHAR(256)    NOT NULL,
    [NormalizedEmail]   NVARCHAR(256)    NOT NULL,
    [EmailConfirmed]    BIT              NOT NULL DEFAULT 0,
    [PasswordHash]      NVARCHAR(MAX)    NULL,
    [SecurityStamp]     NVARCHAR(MAX)    NULL,
    [ConcurrencyStamp]  NVARCHAR(MAX)    NULL,
    [PhoneNumber]       NVARCHAR(20)     NULL,
    [PhoneNumberConfirmed] BIT           NOT NULL DEFAULT 0,
    [TwoFactorEnabled]  BIT              NOT NULL DEFAULT 0,
    [LockoutEnd]        DATETIMEOFFSET   NULL,
    [LockoutEnabled]    BIT              NOT NULL DEFAULT 1,
    [AccessFailedCount] INT              NOT NULL DEFAULT 0,
    [FirstName]         NVARCHAR(100)    NOT NULL,
    [LastName]          NVARCHAR(100)    NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NULL,
    [EmployeeId]        UNIQUEIDENTIFIER NULL,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [LastLoginAt]       DATETIME2        NULL,
    [ProfilePhotoPath]  NVARCHAR(500)    NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Roles
CREATE TABLE [dbo].[Roles] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(256)    NOT NULL,
    [NormalizedName]    NVARCHAR(256)    NOT NULL,
    [ConcurrencyStamp]  NVARCHAR(MAX)    NULL,
    [Description]       NVARCHAR(500)    NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Permissions
CREATE TABLE [dbo].[Permissions] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(100)    NOT NULL,
    [Code]              NVARCHAR(100)    NOT NULL,
    [Module]            NVARCHAR(100)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Permissions_Code] UNIQUE ([Code])
);
GO

-- Role Permissions
CREATE TABLE [dbo].[RolePermissions] (
    [RoleId]            UNIQUEIDENTIFIER NOT NULL,
    [PermissionId]      UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RoleId], [PermissionId])
);
GO

-- User Roles
CREATE TABLE [dbo].[UserRoles] (
    [UserId]            UNIQUEIDENTIFIER NOT NULL,
    [RoleId]            UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserId], [RoleId])
);
GO

-- Refresh Tokens
CREATE TABLE [dbo].[RefreshTokens] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER NOT NULL,
    [Token]             NVARCHAR(500)    NOT NULL,
    [ExpiresAt]         DATETIME2        NOT NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedByIp]       NVARCHAR(50)     NULL,
    [RevokedAt]         DATETIME2        NULL,
    [RevokedByIp]       NVARCHAR(50)     NULL,
    [ReplacedByToken]   NVARCHAR(500)    NULL,
    [IsRevoked]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Employees
CREATE TABLE [dbo].[Employees] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [OrganizationUnitId] UNIQUEIDENTIFIER NULL,
    [ManagerId]         UNIQUEIDENTIFIER NULL,
    [UserId]            UNIQUEIDENTIFIER NULL,
    [PersonnelCode]     NVARCHAR(50)     NOT NULL,
    [NationalCode]      NVARCHAR(10)     NOT NULL,
    [FirstName]         NVARCHAR(100)    NOT NULL,
    [LastName]          NVARCHAR(100)    NOT NULL,
    [FatherName]        NVARCHAR(100)    NULL,
    [BirthDate]         DATE             NULL,
    [Phone]             NVARCHAR(20)     NULL,
    [Email]             NVARCHAR(200)    NULL,
    [Address]           NVARCHAR(500)    NULL,
    [EmploymentDate]    DATE             NOT NULL,
    [ContractEndDate]   DATE             NULL,
    [EmploymentType]    INT              NOT NULL DEFAULT 1, -- 1=Permanent, 2=Contract, 3=Temporary
    [Position]          NVARCHAR(200)    NULL,
    [PhotoPath]         NVARCHAR(500)    NULL,
    [Status]            INT              NOT NULL DEFAULT 1, -- 1=Active, 2=Inactive, 3=Suspended, 4=Terminated
    [Description]       NVARCHAR(1000)   NULL,
    [CurrentScore]      DECIMAL(10,2)    NOT NULL DEFAULT 0,
    [MonthlyScore]      DECIMAL(10,2)    NOT NULL DEFAULT 0,
    [YearlyScore]       DECIMAL(10,2)    NOT NULL DEFAULT 0,
    [Ranking]           INT              NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Employees_PersonnelCode] UNIQUE ([OrganizationId], [PersonnelCode]),
    CONSTRAINT [UQ_Employees_NationalCode] UNIQUE ([OrganizationId], [NationalCode])
);
GO

-- Evaluation Categories
CREATE TABLE [dbo].[EvaluationCategories] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [Name]              NVARCHAR(200)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [Color]             NVARCHAR(20)     NULL,
    [Icon]              NVARCHAR(100)    NULL,
    [Weight]            DECIMAL(5,2)     NOT NULL DEFAULT 1,
    [SortOrder]         INT              NOT NULL DEFAULT 0,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_EvaluationCategories] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Evaluation Items
CREATE TABLE [dbo].[EvaluationItems] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [CategoryId]        UNIQUEIDENTIFIER NOT NULL,
    [Title]             NVARCHAR(200)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [ScoreType]         INT              NOT NULL, -- 1=Positive, 2=Negative
    [DefaultScore]      DECIMAL(10,2)    NOT NULL DEFAULT 0,
    [MaxScore]          DECIMAL(10,2)    NOT NULL DEFAULT 100,
    [MinScore]          DECIMAL(10,2)    NOT NULL DEFAULT -100,
    [Weight]            DECIMAL(5,2)     NOT NULL DEFAULT 1,
    [Color]             NVARCHAR(20)     NULL,
    [Icon]              NVARCHAR(100)    NULL,
    [Priority]          INT              NOT NULL DEFAULT 0,
    [IsAutoCalculated]  BIT              NOT NULL DEFAULT 0,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_EvaluationItems] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Evaluation Rules (Rule Engine)
CREATE TABLE [dbo].[EvaluationRules] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [CategoryId]        UNIQUEIDENTIFIER NULL,
    [ItemId]            UNIQUEIDENTIFIER NULL,
    [Name]              NVARCHAR(200)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [ConditionType]     INT              NOT NULL, -- 1=Delay, 2=Absence, 3=Attendance, 4=Custom
    [Operator]          INT              NOT NULL, -- 1=LessThan, 2=LessOrEqual, 3=Equal, 4=GreaterOrEqual, 5=GreaterThan, 6=Between
    [MinValue]          DECIMAL(10,2)    NULL,
    [MaxValue]          DECIMAL(10,2)    NULL,
    [StringValue]       NVARCHAR(200)    NULL,
    [ScoreImpact]       DECIMAL(10,2)    NOT NULL,
    [Priority]          INT              NOT NULL DEFAULT 0,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_EvaluationRules] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Attendance Logs
CREATE TABLE [dbo].[AttendanceLogs] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EmployeeId]        UNIQUEIDENTIFIER NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [AttendanceDate]    DATE             NOT NULL,
    [EntryTime]         TIME             NULL,
    [ExitTime]          TIME             NULL,
    [WorkingHours]      DECIMAL(5,2)     NULL,
    [OvertimeHours]     DECIMAL(5,2)     NULL,
    [DelayMinutes]      INT              NOT NULL DEFAULT 0,
    [IsAbsent]          BIT              NOT NULL DEFAULT 0,
    [IsOnMission]       BIT              NOT NULL DEFAULT 0,
    [IsOnLeave]         BIT              NOT NULL DEFAULT 0,
    [LeaveType]         NVARCHAR(100)    NULL,
    [Source]            NVARCHAR(50)     NOT NULL DEFAULT 'Sync',
    [ExternalId]        NVARCHAR(100)    NULL,
    [IsProcessed]       BIT              NOT NULL DEFAULT 0,
    [ProcessedAt]       DATETIME2        NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    CONSTRAINT [PK_AttendanceLogs] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_AttendanceLogs_EmployeeDate] UNIQUE ([EmployeeId], [AttendanceDate])
);
GO

-- Attendance Sync Logs
CREATE TABLE [dbo].[AttendanceSyncLogs] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [SyncStartedAt]     DATETIME2        NOT NULL,
    [SyncCompletedAt]   DATETIME2        NULL,
    [Status]            INT              NOT NULL, -- 1=Success, 2=Failed, 3=Partial
    [RecordsProcessed]  INT              NOT NULL DEFAULT 0,
    [RecordsFailed]     INT              NOT NULL DEFAULT 0,
    [ErrorMessage]      NVARCHAR(MAX)    NULL,
    [SourceType]        NVARCHAR(50)     NOT NULL, -- REST, SOAP, SQLView
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_AttendanceSyncLogs] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Employee Scores
CREATE TABLE [dbo].[EmployeeScores] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EmployeeId]        UNIQUEIDENTIFIER NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [CategoryId]        UNIQUEIDENTIFIER NULL,
    [ItemId]            UNIQUEIDENTIFIER NULL,
    [RuleId]            UNIQUEIDENTIFIER NULL,
    [AttendanceLogId]   UNIQUEIDENTIFIER NULL,
    [Score]             DECIMAL(10,2)    NOT NULL,
    [ScoreType]         INT              NOT NULL,
    [Title]             NVARCHAR(200)    NOT NULL,
    [Description]       NVARCHAR(1000)   NULL,
    [ScoreDate]         DATE             NOT NULL,
    [Year]              INT              NOT NULL,
    [Month]             INT              NOT NULL,
    [IsAutoGenerated]   BIT              NOT NULL DEFAULT 0,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_EmployeeScores] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Employee Evaluations (Manual)
CREATE TABLE [dbo].[EmployeeEvaluations] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EmployeeId]        UNIQUEIDENTIFIER NOT NULL,
    [EvaluatorId]       UNIQUEIDENTIFIER NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [CategoryId]        UNIQUEIDENTIFIER NULL,
    [ItemId]            UNIQUEIDENTIFIER NULL,
    [Score]             DECIMAL(10,2)    NOT NULL,
    [ScoreType]         INT              NOT NULL,
    [Notes]             NVARCHAR(2000)   NULL,
    [EvaluationDate]    DATE             NOT NULL,
    [WorkflowStatus]    INT              NOT NULL DEFAULT 1, -- 1=Pending, 2=Approved, 3=Rejected
    [ApprovedBy]        UNIQUEIDENTIFIER NULL,
    [ApprovedAt]        DATETIME2        NULL,
    [ApprovalComments]  NVARCHAR(1000)   NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_EmployeeEvaluations] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Attachments
CREATE TABLE [dbo].[Attachments] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EntityType]        NVARCHAR(100)    NOT NULL,
    [EntityId]          UNIQUEIDENTIFIER NOT NULL,
    [FileName]          NVARCHAR(255)    NOT NULL,
    [OriginalFileName]  NVARCHAR(255)    NOT NULL,
    [ContentType]       NVARCHAR(100)    NOT NULL,
    [FileSize]          BIGINT           NOT NULL,
    [FilePath]          NVARCHAR(500)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [UploadedBy]        UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Attachments] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Notifications
CREATE TABLE [dbo].[Notifications] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NULL,
    [Title]             NVARCHAR(200)    NOT NULL,
    [Message]           NVARCHAR(1000)   NOT NULL,
    [Type]              INT              NOT NULL, -- 1=Info, 2=Warning, 3=Alert, 4=Success
    [Category]          NVARCHAR(100)    NULL,
    [EntityType]        NVARCHAR(100)    NULL,
    [EntityId]          UNIQUEIDENTIFIER NULL,
    [IsRead]            BIT              NOT NULL DEFAULT 0,
    [ReadAt]            DATETIME2        NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Appeals
CREATE TABLE [dbo].[Appeals] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EmployeeId]        UNIQUEIDENTIFIER NOT NULL,
    [ScoreId]           UNIQUEIDENTIFIER NULL,
    [EvaluationId]      UNIQUEIDENTIFIER NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [Reason]            NVARCHAR(2000)   NOT NULL,
    [Status]            INT              NOT NULL DEFAULT 1, -- 1=Pending, 2=Approved, 3=Rejected
    [ReviewedBy]        UNIQUEIDENTIFIER NULL,
    [ReviewedAt]        DATETIME2        NULL,
    [ReviewComments]    NVARCHAR(1000)   NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Appeals] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Audit Logs
CREATE TABLE [dbo].[AuditLogs] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER NULL,
    [UserName]          NVARCHAR(256)    NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NULL,
    [Action]            NVARCHAR(100)    NOT NULL,
    [EntityType]        NVARCHAR(100)    NULL,
    [EntityId]          NVARCHAR(100)    NULL,
    [OldValues]         NVARCHAR(MAX)    NULL,
    [NewValues]         NVARCHAR(MAX)    NULL,
    [IpAddress]         NVARCHAR(50)     NULL,
    [UserAgent]         NVARCHAR(500)    NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Settings
CREATE TABLE [dbo].[Settings] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NULL,
    [Key]               NVARCHAR(100)    NOT NULL,
    [Value]             NVARCHAR(MAX)    NOT NULL,
    [Category]          NVARCHAR(100)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [DataType]          NVARCHAR(50)     NOT NULL DEFAULT 'string',
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Settings_Key_Org] UNIQUE ([OrganizationId], [Key])
);
GO

-- Holidays
CREATE TABLE [dbo].[Holidays] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [Title]             NVARCHAR(200)    NOT NULL,
    [HolidayDate]       DATE             NOT NULL,
    [IsRecurring]       BIT              NOT NULL DEFAULT 0,
    [Description]       NVARCHAR(500)    NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Holidays] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Employee Timeline
CREATE TABLE [dbo].[EmployeeTimelines] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [EmployeeId]        UNIQUEIDENTIFIER NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [EventType]         INT              NOT NULL,
    [Title]             NVARCHAR(200)    NOT NULL,
    [Description]       NVARCHAR(1000)   NULL,
    [EntityType]        NVARCHAR(100)    NULL,
    [EntityId]          UNIQUEIDENTIFIER NULL,
    [EventDate]         DATETIME2        NOT NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_EmployeeTimelines] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Rankings
CREATE TABLE [dbo].[Rankings] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [EntityType]        NVARCHAR(50)     NOT NULL, -- Employee, Department, Manager
    [EntityId]          UNIQUEIDENTIFIER NOT NULL,
    [Rank]              INT              NOT NULL,
    [Score]             DECIMAL(10,2)    NOT NULL,
    [Year]              INT              NOT NULL,
    [Month]             INT              NULL,
    [PeriodType]        INT              NOT NULL, -- 1=Monthly, 2=Yearly
    [CalculatedAt]      DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Rankings] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Alert Rules
CREATE TABLE [dbo].[AlertRules] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [Name]              NVARCHAR(200)    NOT NULL,
    [AlertType]         INT              NOT NULL,
    [Threshold]         DECIMAL(10,2)    NULL,
    [Condition]         NVARCHAR(500)    NULL,
    [NotifyEmployee]    BIT              NOT NULL DEFAULT 1,
    [NotifyManager]     BIT              NOT NULL DEFAULT 1,
    [NotifyAdmin]       BIT              NOT NULL DEFAULT 0,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_AlertRules] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- Attendance Integration Settings
CREATE TABLE [dbo].[AttendanceIntegrationSettings] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [SourceType]        NVARCHAR(50)     NOT NULL, -- REST, SOAP, SQLView
    [EndpointUrl]       NVARCHAR(500)    NULL,
    [ConnectionString]  NVARCHAR(500)    NULL,
    [SqlViewName]       NVARCHAR(200)    NULL,
    [ApiKey]            NVARCHAR(500)    NULL,
    [Username]          NVARCHAR(200)    NULL,
    [Password]          NVARCHAR(500)    NULL,
    [SyncIntervalMinutes] INT            NOT NULL DEFAULT 5,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [LastSyncAt]        DATETIME2        NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NULL,
    CONSTRAINT [PK_AttendanceIntegrationSettings] PRIMARY KEY CLUSTERED ([Id])
);
GO

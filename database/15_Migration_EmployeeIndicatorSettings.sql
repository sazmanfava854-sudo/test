USE [HRPerformanceDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmployeeIndicatorSettings')
BEGIN
    CREATE TABLE [dbo].[EmployeeIndicatorSettings] (
        [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [EmployeeId]    UNIQUEIDENTIFIER NOT NULL,
        [CategoryId]    UNIQUEIDENTIFIER NOT NULL,
        [Weight]        DECIMAL(5,2)     NOT NULL DEFAULT 1,
        [IsActive]      BIT              NOT NULL DEFAULT 1,
        [CreatedAt]     DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt]     DATETIME2        NULL,
        [CreatedBy]     UNIQUEIDENTIFIER NULL,
        [UpdatedBy]     UNIQUEIDENTIFIER NULL,
        [IsDeleted]     BIT              NOT NULL DEFAULT 0,
        CONSTRAINT [PK_EmployeeIndicatorSettings] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_EmployeeIndicatorSettings_EmployeeCategory] UNIQUE ([EmployeeId], [CategoryId])
    );

    ALTER TABLE [dbo].[EmployeeIndicatorSettings]
        ADD CONSTRAINT [FK_EIS_Employee] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]);
    ALTER TABLE [dbo].[EmployeeIndicatorSettings]
        ADD CONSTRAINT [FK_EIS_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[EvaluationCategories]([Id]);
END
GO

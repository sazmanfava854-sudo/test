-- دسترسی گروهی به ماژول شاهکار (روی پایگاه AppAuth / RayvarzRuleEngine)
IF OBJECT_ID(N'dbo.AppUserGroup', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUserGroup', N'CanAccessShahkar') IS NULL
    ALTER TABLE dbo.AppUserGroup ADD CanAccessShahkar BIT NOT NULL
        CONSTRAINT DF_AppUserGroup_Shahkar DEFAULT (0);

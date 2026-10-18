IF OBJECT_ID(N'dbo.AppUserGroup', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUserGroup', N'CanAccessCostCenter') IS NULL
    ALTER TABLE dbo.AppUserGroup ADD CanAccessCostCenter BIT NOT NULL
        CONSTRAINT DF_AppUserGroup_CostCenter DEFAULT (0);

IF OBJECT_ID(N'dbo.AppUser', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUser', N'DirectCanAccessCostCenter') IS NULL
    ALTER TABLE dbo.AppUser ADD DirectCanAccessCostCenter BIT NOT NULL
        CONSTRAINT DF_AppUser_DirectCostCenter DEFAULT (0);

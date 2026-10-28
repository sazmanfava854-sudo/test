-- دسترسی جداگانه به تب «تغییر تاریخ چک» (Installment_List.PaymentDate)
IF OBJECT_ID(N'dbo.AppUserGroup', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUserGroup', N'CanAccessInstallmentCheckDate') IS NULL
    ALTER TABLE dbo.AppUserGroup ADD CanAccessInstallmentCheckDate BIT NOT NULL
        CONSTRAINT DF_AppUserGroup_InstallmentCheckDate DEFAULT (0);

IF OBJECT_ID(N'dbo.AppUser', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUser', N'DirectCanAccessInstallmentCheckDate') IS NULL
    ALTER TABLE dbo.AppUser ADD DirectCanAccessInstallmentCheckDate BIT NOT NULL
        CONSTRAINT DF_AppUser_DirectInstallmentCheckDate DEFAULT (0);

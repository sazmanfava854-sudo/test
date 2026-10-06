-- دسترسی‌های شخصی کاربر (بدون گروه) — روی پایگاه AppAuth / RayvarzRuleEngine
IF OBJECT_ID(N'dbo.AppUser', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUser', N'DirectCanAccessUnsentFiches') IS NULL
    ALTER TABLE dbo.AppUser ADD
        DirectCanAccessUnsentFiches BIT NOT NULL CONSTRAINT DF_AppUser_DirectUnsent DEFAULT (0),
        DirectCanAccessInstallment BIT NOT NULL CONSTRAINT DF_AppUser_DirectInstallment DEFAULT (0),
        DirectCanAccessFicheDateChange BIT NOT NULL CONSTRAINT DF_AppUser_DirectFicheDate DEFAULT (0),
        DirectCanAccessBankInquiryConfirm BIT NOT NULL CONSTRAINT DF_AppUser_DirectBankInquiry DEFAULT (0),
        DirectCanAccessShahkar BIT NOT NULL CONSTRAINT DF_AppUser_DirectShahkar DEFAULT (0),
        DirectCanManageUsers BIT NOT NULL CONSTRAINT DF_AppUser_DirectUsers DEFAULT (0);

-- ============================================================
-- تشخیص: آیا RayvarzRuleEngine درست ساخته شده؟
-- این فایل را روی سرور 232 در SSMS اجرا کنید.
-- ============================================================

PRINT '=== 1) وجود دیتابیس RayvarzRuleEngine ===';
SELECT name, create_date FROM sys.databases WHERE name = N'RayvarzRuleEngine';

PRINT '=== 2) جداول داخل RayvarzRuleEngine ===';
USE RayvarzRuleEngine;
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

PRINT '=== 3) RuleSyncState ===';
IF OBJECT_ID(N'dbo.RuleSyncState', N'U') IS NOT NULL
    SELECT * FROM dbo.RuleSyncState;
ELSE
    PRINT 'ERROR: dbo.RuleSyncState وجود ندارد — 01_RayvarzRuleEngine_Schema.sql را اجرا کنید';

PRINT '=== 4) Golden fiches (باید 4 باشد) ===';
IF OBJECT_ID(N'dbo.RuleGoldenFiche', N'U') IS NOT NULL
    SELECT GoldenFicheId, Name, FicheNo FROM dbo.RuleGoldenFiche ORDER BY GoldenFicheId;
ELSE
    PRINT 'ERROR: dbo.RuleGoldenFiche وجود ندارد — 02_RuleGolden_Seed.sql را اجرا کنید';

PRINT '=== 5) اشتباه رایج: جداول در DbRuleEngein ساخته شده؟ ===';
USE DbRuleEngein;
IF OBJECT_ID(N'dbo.RuleSyncState', N'U') IS NOT NULL
    PRINT 'WARNING: RuleSyncState در DbRuleEngein هم وجود دارد — ConnectionStrings:RayvarzRuleEngine نباید به DbRuleEngein اشاره کند';
ELSE
    PRINT 'OK: RuleSyncState در DbRuleEngein نیست (درست است)';

USE RayvarzRuleEngine;

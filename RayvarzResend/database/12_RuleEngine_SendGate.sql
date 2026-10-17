-- Rule Engine SendGate — ارسال به رایورز از لحظه Detected بسته است تا مدیر تأیید کند.
-- برنامه همین ALTERها را در اولین اتصال اجرا می‌کند (RuleEngineStateRepository.SendGateSchemaSql)؛
-- اگر کاربر SQL برنامه مجوز ALTER ندارد، این اسکریپت را دستی اجرا کنید. idempotent است.
--
-- RuleSyncState.SendGate              VARCHAR(20) NULL   → Open | Blocked (NULL = Open)
-- RuleSyncState.ApprovedSendXmlHash   CHAR(64) NULL      → هش XmlBody تأییدشده توسط مدیر
-- RuleSyncState.SendGateChangedAtUtc  DATETIME2(3) NULL
-- RuleSyncState.SendGateChangedBy     NVARCHAR(200) NULL → rule-sync | نام کاربری مدیر | schema-12
--
-- مقداردهی اولیه: ApprovedSendXmlHash = LastStableXmlHash؛ اگر candidate در انتظار (Detected) باشد → Blocked.

USE RayvarzRuleEngine;
GO

IF COL_LENGTH(N'dbo.RuleSyncState', N'ApprovedSendXmlHash') IS NULL
    ALTER TABLE dbo.RuleSyncState ADD ApprovedSendXmlHash CHAR(64) NULL;
IF COL_LENGTH(N'dbo.RuleSyncState', N'SendGateChangedAtUtc') IS NULL
    ALTER TABLE dbo.RuleSyncState ADD SendGateChangedAtUtc DATETIME2(3) NULL;
IF COL_LENGTH(N'dbo.RuleSyncState', N'SendGateChangedBy') IS NULL
    ALTER TABLE dbo.RuleSyncState ADD SendGateChangedBy NVARCHAR(200) NULL;
IF COL_LENGTH(N'dbo.RuleSyncState', N'SendGate') IS NULL
BEGIN
    ALTER TABLE dbo.RuleSyncState ADD SendGate VARCHAR(20) NULL;
    EXEC (N'
        UPDATE s SET
            ApprovedSendXmlHash = s.LastStableXmlHash,
            SendGate = CASE WHEN EXISTS (
                SELECT 1 FROM dbo.RuleCandidate c
                WHERE c.NidMember = s.NidMember AND c.Status IN (''Detected'', ''Parsed'', ''Validated''))
                THEN ''Blocked'' ELSE ''Open'' END,
            SendGateChangedAtUtc = SYSUTCDATETIME(),
            SendGateChangedBy = N''schema-12''
        FROM dbo.RuleSyncState s;');
END
GO

-- وضعیت فعلی:
SELECT NidMember, SendGate, LEFT(ApprovedSendXmlHash, 12) AS ApprovedHash, LEFT(LastStableXmlHash, 12) AS StableHash,
       SendGateChangedAtUtc, SendGateChangedBy
FROM dbo.RuleSyncState;
GO

-- باز کردن ارسال توسط مدیر: از UI مدیریت یا POST /api/rule/send-gate/approve استفاده کنید
-- (هش فعلی Member را ثبت می‌کند تا چرخه بعدی دوباره Blocked نکند).

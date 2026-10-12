-- Rule Engine sync — مرجع اسکیما (اجرا نکنید اگر 01 و 03 قبلاً اجرا شده‌اند).
-- جداول از database/01_RayvarzRuleEngine_Schema.sql + 03_Phase4_CircuitBreaker.sql (تاریخچه git: 669c8b2 و cab9a87).
-- Member + MemberHistory روی DbRuleEngein (ConnectionStrings:RuleEngine) — فقط خواندن.
--
-- قیدهایی که کد sync رعایت می‌کند:
--   RuleSyncState.LastStableXmlHash      CHAR(64) NULL      → NULL به‌جای '' (پدینگ CHAR)
--   RuleSyncState.ActiveDslVersion       INT NOT NULL       → هرگز NULL نوشته نمی‌شود
--   RuleCandidate.CandidateId            BIGINT IDENTITY    → OUTPUT INSERTED.CandidateId
--   RuleCandidate UNIQUE (NidMember, CanonicalXmlHash)      → هش تکراری = reopen همان ردیف
--   RuleCandidate.SourceNidHistory / SourceModifyAt / StableEligibleAtUtc  NOT NULL
--   RuleCandidate.Status                 VARCHAR(30)        → Detected | Stable | Promoted | Rejected
--   RulePromotionLog.Action              VARCHAR(30)        → Baseline | Superseded | Stable | Promoted | Rejected
--   RuleDryRunResult.GoldenFicheId       INT NOT NULL       → sync فعلاً ردیف dry-run ساختگی نمی‌نویسد

USE RayvarzRuleEngine;
GO

-- بررسی سریع وضعیت sync:
SELECT * FROM dbo.RuleSyncState;
SELECT CandidateId, SourceNidHistory, SourceModifyAt, Status, RejectReason, StableEligibleAtUtc
FROM dbo.RuleCandidate ORDER BY CandidateId DESC;
SELECT TOP 50 * FROM dbo.RulePromotionLog ORDER BY LogId DESC;
GO

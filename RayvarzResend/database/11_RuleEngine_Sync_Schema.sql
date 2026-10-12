-- Rule Engine sync tables (RayvarzRuleEngine)
-- اگر جداول از قبل با ستون‌های زیر وجود دارند، این اسکریپت را اجرا نکنید — اپ فقط همان اسکیمای موجود را می‌خواند.
-- Member + MemberHistory روی DbRuleEngein (ConnectionStrings:RuleEngine).

USE RayvarzRuleEngine;
GO

-- مرجع: اسکیمای فعلی production (ستون‌های SELECT کاربر)

/*
RuleSyncState:
  NidMember, NidClass, LastSeenNidHistory, LastSeenModifyAt,
  LastStableNidHistory, LastStableModifyAt, LastStableXmlHash,
  ActiveDslVersion, ActiveEngine, ActiveSnapshotId, UpdatedAtUtc,
  ConsecutiveDynamicFailures, CircuitBreakerOpenUntilUtc

RuleCandidate:
  CandidateId, NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
  Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc, FirstSeenAtUtc, LastCheckedAtUtc

RulePromotionLog:
  LogId, NidMember, CandidateId, SnapshotId, Action, Reason, CreatedAtUtc

RuleDryRunResult:
  DryRunId, CandidateId, SnapshotId, GoldenFicheId, EngineName, Success, ErrorMessage, OutputJson, ExecutedAtUtc
*/

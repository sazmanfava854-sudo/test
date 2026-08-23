-- =============================================================================
-- Rayvarz Resend — دسترسی‌های SQL موردنیاز اپلیکیشن
-- دیتابیس Sara8M03 (ConnectionStrings:Sara) + Rayvarz + RuleEngine (اختیاری)
--
-- قبل از اجرا: نام Login/User اپ را جایگزین [RayvarzResendApp] کنید.
-- خطای فعلی «دسترسی به Base_NosaziCode» با GRANT SELECT روی این جدول رفع می‌شود.
-- =============================================================================

-- --- Sara8M03 (Sara) — چک خزانه + ارسال به رایورز --------------------------------

-- کد نوسازی (JOIN در پیش‌نمایش چک خزانه، بارگذاری فیش درآمد، جستجوی جمعی با منطقه، نوسازی duty)
GRANT SELECT ON dbo.Base_NosaziCode TO [RayvarzResendApp];

-- چک خزانه — SELECT
GRANT SELECT ON dbo.Income TO [RayvarzResendApp];
GRANT SELECT ON dbo.Income_Fiche TO [RayvarzResendApp];
GRANT SELECT ON dbo.Installment TO [RayvarzResendApp];
GRANT SELECT ON dbo.Installment_List TO [RayvarzResendApp];
GRANT SELECT ON dbo.Sh_RequestInfo TO [RayvarzResendApp];

-- چک خزانه — UPDATE (ثبت وضعیت خزانه روی ردیف تقسیط)
GRANT UPDATE ON dbo.Installment_List TO [RayvarzResendApp];

-- ارسال به رایورز — SELECT (فیش درآمد / نوسازی / صنفی)
GRANT SELECT ON dbo.Income_Calculation TO [RayvarzResendApp];
GRANT SELECT ON dbo.CI_IncomeCalculation TO [RayvarzResendApp];
GRANT SELECT ON dbo.Income_OddmentAccount TO [RayvarzResendApp];
GRANT SELECT ON dbo.Duty_Fiche TO [RayvarzResendApp];
GRANT SELECT ON dbo.Duty_FicheSub TO [RayvarzResendApp];
GRANT SELECT ON dbo.Duty_OddmentAccount TO [RayvarzResendApp];
GRANT SELECT ON dbo.Accounting_DocHeader TO [RayvarzResendApp];
GRANT SELECT ON dbo.Accounting_DocNotSent TO [RayvarzResendApp];

-- ارسال به رایورز — INSERT (پس از ارسال موفق SOAP؛ وقتی AccountingDoc:DryRun=false)
GRANT INSERT ON dbo.Accounting_DocHeader TO [RayvarzResendApp];
GRANT INSERT ON dbo.Accounting_DocDetails TO [RayvarzResendApp];

-- --- احراز هویت اپ (ConnectionStrings:AppAuth یا fallback Sara) --------------------
-- اگر جداول در همان Sara8M03 هستند، GRANTها را در همان DB بدهید.
-- EnsureSchemaAsync در اولین اجرا ممکن است CREATE TABLE بخواهد (یا اسکریپت 07/08 را اجرا کنید).

GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.AppUser TO [RayvarzResendApp];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.AppUserGroup TO [RayvarzResendApp];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.AppUserGroupMember TO [RayvarzResendApp];

-- --- Rayvarz (ConnectionStrings:Rayvarz) — تأیید وجود سند و متادیتا ----------------

GRANT SELECT ON ray.incmdocsys TO [RayvarzResendApp];

-- --- RuleEngine (ConnectionStrings:RuleEngine — اختیاری؛ یا RuleEngine:LocalXmlPath) -

-- GRANT SELECT ON dbo.Member TO [RayvarzResendApp];  -- در دیتابیس DbRuleEngein

# فاز ۰ — راه‌اندازی RayvarzRuleEngine

## ۱. اجرای اسکریپت‌ها روی سرور ۲۳۲

```sql
-- 1) ساخت DB و جداول
:r database\01_RayvarzRuleEngine_Schema.sql

-- 2) seed فیش‌های golden
:r database\02_RuleGolden_Seed.sql
```

## ۲. تنظیم appsettings

کپی `appsettings.Development.json.example` → `appsettings.Development.json` و پر کردن:

- `ConnectionStrings:Sara` → Sara8M03
- `ConnectionStrings:RuleEngine` → DbRuleEngein (read-only Member/History)
- `ConnectionStrings:RayvarzRuleEngine` → RayvarzRuleEngine

## ۳. تست API

| Endpoint | کار |
|----------|-----|
| `GET /api/db-test` | اتصال Sara، Rayvarz، DbRuleEngein، RayvarzRuleEngine |
| `GET /api/rule/sync/state` | وضعیت sync |
| `POST /api/rule/sync/run` | sync دستی MemberHistory |
| `GET /api/rule/history/latest` | آخرین NidHistory |
| `GET /api/rule/golden` | لیست golden + expected rows |
| `POST /api/rule/golden/dry-run` | تست live از Sara + مقایسه Legacy |

## ۴. رفتار تولید

`Rayvarz:PayloadSource` = `LegacyCSharp` (بدون تغییر نسبت به v16).

## عیب‌یابی

### `Invalid object name 'dbo.RuleSyncState'`

**علت:** اتصال SQL برقرار است ولی دیتابیس `RayvarzRuleEngine` و جداول آن ساخته نشده.

**راه‌حل:**

1. در SSMS به **سرور 232** وصل شوید
2. فایل `database/01_RayvarzRuleEngine_Schema.sql` را اجرا کنید (کل فایل)
3. سپس `database/02_RuleGolden_Seed.sql`
4. در `appsettings.Development.json` مطمئن شوید:

```json
"RuleEngine": "Server=232;Database=DbRuleEngein;...",
"RayvarzRuleEngine": "Server=232;Database=RayvarzRuleEngine;..."
```

| Connection | Database |
|------------|----------|
| `RuleEngine` | **DbRuleEngein** (خواندن Member/History) |
| `RayvarzRuleEngine` | **RayvarzRuleEngine** (state + golden) |

**اشتباه رایج:** هر دو را روی `DbRuleEngein` گذاشتن — جدول `RuleSyncState` آنجا وجود ندارد.

**تأیید:**
```sql
USE RayvarzRuleEngine;
SELECT * FROM dbo.RuleSyncState;
SELECT COUNT(*) FROM dbo.RuleGoldenFiche;  -- باید 4 باشد
```

تا زمان اجرای اسکریپت‌ها، برنامه بالا می‌آید ولی rule sync فقط warning می‌دهد (crash نمی‌کند).

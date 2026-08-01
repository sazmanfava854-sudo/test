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

Background sync هر ۱۵ دقیقه (قابل غیرفعال با `RuleEngine:EnableBackgroundSync=false`).

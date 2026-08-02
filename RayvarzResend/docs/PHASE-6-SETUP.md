# فاز ۶ — گسترش Parser + توابع درآمدی (iNcOME*)

پیش‌نیاز: فاز ۴ (Promote + Dynamic فعال برای Nosazi).

## هدف

| # | کار | وضعیت |
|---|-----|--------|
| 6.1 | Parser: `iNcOME`, `iNcOMEOragh`, `iNcOMESeprdeh`, `iNcOMEEshghal`, … | ✅ |
| 6.2 | Operation: `Income.BuildIncomeRows` | ✅ |
| 6.3 | DryRun: skip بدنه VB درآمد → ردیف از فیش live Sara | ✅ |
| 6.4 | Golden Income (اختیاری) | ⏳ در صورت نیاز seed جدا |

## الگوی اجرا (مثل Nosazi)

```
Run()
  If DutyFicheResultList.Count > 0 → Nosazi()
  ElseIf IncomeFicheResultList.Count > 0 → iNcOME()
        │
        ▼ DryRun
  Income.BuildIncomeRows  ← کپی Fiche.Rows از Income_Calculation (Sara live)
  Validate.RowSumEqualsPayable
```

بدنه کامل VB داخل `iNcOME*` (مثل Nosazi) در DryRun اجرا نمی‌شود؛ ردیف‌ها از `FicheRepository` (جدول `Income_Calculation`) می‌آیند.

## توابع پشتیبانی‌شده

| تابع | نقش |
|------|-----|
| `Run` | EntryPoint / dispatch |
| `Nosazi` | نوسازی / صنفی (Duty) |
| `iNcOME` | درآمد اصلی |
| `iNcOMEOragh` | اوراق |
| `iNcOMESeprdeh` | سپرده |
| `iNcOMEEshghal` | اشغال |
| هر `iNcOME*` | با پیشوند `iNcOME` هم پشتیبانی می‌شود |

`ParserVersion` → **2.1.0**

## تست بعد از deploy

```powershell
# 1) rebuild snapshot با parser جدید
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/dsl/parse?force=true"
# انتظار: iNcOME در unsupportedFunctions نباشد (یا کمتر)

# 2) preview یک فیش درآمدی از UI / API

# 3) Duty طلایی همچنان سبز بماند
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/golden/dry-run" |
  Select-Object engineName, passed, allPassed
```

## Golden درآمد (نمونه‌های کاربر — فاز ۶)

فایل SQL:

```text
database/04_RuleGolden_Seed_Phase6_Samples.sql
```

روی `RayvarzRuleEngine` (سرور ۲۳۲) بعد از `02_RuleGolden_Seed.sql` اجرا کنید.

| Id | FicheNo | نوع | ردیف | Payable |
|----|---------|-----|------|---------|
| 5 | `050733453546` | Income شهرسازی | 5 | 5,379,066,000 |
| 6 | `050733451977` | Income شهرسازی | 3 | 2,024,365,000 |
| 7 | `050733447710` | Income شهرسازی | 5 | 1,780,716,000 |
| 8 | `050733454216` | Income شهرسازی | 4 | 147,291,000 |
| 9 | `071105/0385826` | Duty نوسازی | 4 | 38,688,000 |
| 10 | `071205/20381801` | Duty صنفی | 3 | 8,089,000 |

منبع expected: `Ray_CityHall.ray.incmdocsys` — فیش‌ها در Sara (`Income_Fiche` / `Duty_Fiche`) موجودند.

```powershell
# بعد از اجرای SQL:
Invoke-RestMethod -Uri "http://localhost:5000/api/rule/golden"
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/golden/dry-run" |
  Select-Object engineName, total, passed, allPassed
# انتظار با Dynamic: total>=10 ، و income+duty سبز اگر ردیف‌های Sara با Rayvarz هم‌خوان باشند
```

`GoldenDryRunService` حالا `FicheCategory.Income` را می‌پذیرد.

## ایمنی

- `PayloadSource=LegacyCSharp` و `DryRun` را طبق سیاست خود نگه دارید
- بعد از `dsl/parse?force=true` در صورت نیاز دوباره `promote/run?force=true`
- Rollback: `POST /api/rule/promote/rollback`

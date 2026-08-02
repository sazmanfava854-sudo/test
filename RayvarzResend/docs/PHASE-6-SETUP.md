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

## Golden درآمد (اختیاری)

فعلاً seed فقط ۴ فیش Duty دارد. برای golden درآمدی:

1. چند `FicheNo` درآمدی پایدار از Sara انتخاب کنید
2. expected را از `ray.incmdocsys` بگیرید
3. به `02_RuleGolden_Seed.sql` اضافه کنید (`Scenario` مثلاً `Income`)

`GoldenDryRunService` حالا `FicheCategory.Income` را می‌پذیرد.

## ایمنی

- `PayloadSource=LegacyCSharp` و `DryRun` را طبق سیاست خود نگه دارید
- بعد از `dsl/parse?force=true` در صورت نیاز دوباره `promote/run?force=true`
- Rollback: `POST /api/rule/promote/rollback`

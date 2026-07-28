# نقطه بازگشت — RayvarzResend نسخه ۱۶ (C# ثابت / nosazo)

این سند مشخص می‌کند چطور هر زمان به **همان تحویلی که منطق nosazo در C# پیاده شده** برگردید (قبل از معماری Rule Engine از دیتابیس).

## شناسه‌ها

| نوع | مقدار |
|-----|--------|
| Git commit | `766da11` (شاخه `rayvarz-resend`) |
| Git tag | `rayvarz-resend-baseline-v16` |
| شاخه آرشیو | `baseline/rayvarz-resend-v16` |
| Zip | `RayvarzResend-16.zip` (ریشه مخزن) |

## بازگردانی از Git

```bash
git fetch origin
git checkout baseline/rayvarz-resend-v16
# یا فقط همان commit:
git checkout rayvarz-resend-baseline-v16
```

## بازگردانی از Zip (بدون Git)

1. دانلود:  
   `https://github.com/sazmanfava854-sudo/test/raw/baseline/rayvarz-resend-v16/RayvarzResend-16.zip`  
   (یا از tag / commit بالا)
2. استخراج و اجرا: `cd RayvarzResend\RayvarzResend.Web` → `dotnet run`

## محتوای این baseline

- منطق نوسازی از **nosazo.vb** در `DutyNosaziLogic` / `DutyDistrictBranchResolver` / `FicheRepository`
- SOAP ITC (`SoapServices`, addressing, enum names)
- بدون خواندن `DbRuleEngein.dbo.Member`

برای معمازی جدید (Rule Engine) روی شاخه `rayvarz-resend` ادامه دهید؛ این baseline دست‌نخورده می‌ماند.

# تحویل — RayvarzResend نسخه ۲۱

نسخه **v21** بر پایه **v20** (همه باگ‌های ۱–۲۰) + **۴ رفع بحرانی**.

## دریافت

| روش | مسیر |
|-----|------|
| Zip | `RayvarzResend-21.zip` |
| Git tag | `rayvarz-resend-v21` |
| شاخه | `rayvarz-resend` |

`GET /api/config` → `releaseVersion: 21`

## رفع‌های بحرانی v21

| # | موضوع | فایل |
|---|--------|------|
| C1 | Fund انتخابی UI دیگر توسط `SuggestedFund` بازنویسی نمی‌شود | `SoapServices.cs` |
| C2 | تاریخ‌های ویرایش‌شده در فرم بر تاریخ DB اولویت دارند | `SoapServices.cs` |
| C3 | شکست ارسال ۱۵۷ → ۱۵۸ ارسال نمی‌شود (`PairAborted`) | `TahatorResendService.cs` |
| C4 | تهاتر ۱۵۸ بدون منطقه → خطا (نه fallback خاموش به Branch 102) | `TahatorRowBuilder.ResolveSendBranch` |

## تست

```bash
cd RayvarzResend
dotnet test                    # 171 تست
node scripts/Bug14ValSummaryTests.mjs
```

## باگ‌های v20 (همچنان شامل)

باگ‌های ۱–۲۰ (به‌جز ۲ و ۱۶) — جزئیات در `DELIVERY-v20.md`.

## باقی‌مانده (غیربحرانی — v22+)

- RowDate تهاتر از تاریخ پرداخت فیش (نه امروز)
- PaymentBranch خالی → Bank=0 (نه 18)
- تاریخ ارقام فارسی در DateHelper
- Fund منطقه ۹ در UI config

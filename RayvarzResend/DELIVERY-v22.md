# تحویل — RayvarzResend نسخه ۲۲

نسخه **v22** بر پایه **v21** (همه باگ‌های ۱–۲۰ + ۴ رفع بحرانی) + **۱۰ رفع غیربحرانی**.

## دریافت

| روش | مسیر |
|-----|------|
| Zip | `RayvarzResend-22.zip` |
| Git tag | `rayvarz-resend-v22` |
| شاخه | `rayvarz-resend` |

`GET /api/config` → `releaseVersion: 22`

## رفع‌های غیربحرانی v22

| # | موضوع | فایل |
|---|--------|------|
| N1 | ارقام فارسی/عربی در تاریخ (`DateHelper.ToRayvarzDate`) | `Helpers.cs` |
| N2 | پارس اعشاری legacy (`123.45` → 123 نه 12345) | `Helpers.cs` |
| N3 | `ResolvePaymentDateByStatus`: وضعیت ۳ → PaymentDate (VB parity) | `FicheDateResolver.cs` |
| N4 | RowDate تهاتر از تاریخ پرداخت (نه امروز) | `FicheDateResolver.cs` |
| N5 | PaymentBranch خالی → null (نه پیش‌فرض 18) | `FicheRepository.cs` |
| N6 | Fund منطقه ۹ در config: `200209008` (بانک 18) | `Program.cs`, `appsettings.json` |
| N7 | تهاتر بدون گروه → پیش‌فرض 157 | `TahatorRowBuilder.cs` |
| N8 | خطای SQL چک رایورز تهاتر → `Warning` در پاسخ | `TahatorResendService.cs`, `Models.cs` |
| N9 | دکمه ارسال UI از `canSend` / `blockReason` سرور پیروی می‌کند | `app.js` |
| N10 | نیک‌نام نوسازی: SQL منطقه‌محور (۷ بخش، بدون prefix شهر) | `FicheRepository.cs` |

## تست

```bash
cd RayvarzResend
dotnet test                    # 178 تست
node scripts/Bug14ValSummaryTests.mjs
```

## نسخه‌های قبلی

- v21: رفع‌های بحرانی — [`DELIVERY-v21.md`](DELIVERY-v21.md)
- v20: باگ‌های ۱–۲۰ — [`DELIVERY-v20.md`](DELIVERY-v20.md)

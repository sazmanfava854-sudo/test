# تحویل — RayvarzResend نسخه ۲۳

نسخه **v23** بر پایه **v22** (همه باگ‌های ۱–۲۰ + بحرانی v21 + غیربحرانی v22) + **رفع باگ جزئی ۱۳**.

## دریافت

| روش | مسیر |
|-----|------|
| Zip | `RayvarzResend-23.zip` |
| Git tag | `rayvarz-resend-v23` |
| شاخه | `rayvarz-resend` |

`GET /api/config` → `releaseVersion: 23`

## رفع v23

| # | موضوع | فایل |
|---|--------|------|
| 13 | outage دیتابیس Rayvarz → `NeedsSend` دیگر `true` نمی‌شود؛ `Warning` برمی‌گردد | `TahatorResendService.cs`, `TahatorSendPolicy.cs` |

## تست

```bash
cd RayvarzResend
dotnet test                    # 179 تست
node scripts/Bug14ValSummaryTests.mjs
```

## نسخه‌های قبلی

- v22: غیربحرانی — [`DELIVERY-v22.md`](DELIVERY-v22.md)
- v21: بحرانی — [`DELIVERY-v21.md`](DELIVERY-v21.md)
- v20: باگ‌های ۱–۲۰ — [`DELIVERY-v20.md`](DELIVERY-v20.md)

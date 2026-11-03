# تحویل — RayvarzResend **v36** (تهاتر درآمدی — کد درآمد رایورز)

شاخه: [`cursor/tahator-income-dtl-84f2`](https://github.com/sazmanfava854-sudo/test/tree/cursor/tahator-income-dtl-84f2)

Tag: `rayvarzresend-tahator-dtl-v36`

`GET /api/config` → `releaseVersion: 36`

## دریافت

[دانلود RayvarzResend-36.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-36.zip)

## تغییرات نسبت به v35

- **نگاشت `IncmNo` از `CI_IncomeCalculation.IncomeCode`** (مثلاً 1271 → 1270) به‌جای استفاده از شناسه محاسبه
- **فیلتر `ADP_IncomeAccountGroup` برای گروه ۱۵۸**
- **اولویت ردیف‌های `Accounting_DocDetails`** اگر برای فیش از قبل در Sara ذخیره شده باشد
- **Tahator builder**: oddment، مقیاس به `Payable`، ادغام ردیف‌های تکراری `IncmNo`
- شامل قابلیت‌های v35 (DTL تهاتر درآمدی) و v34 (مرکز هزینه و …)

## نسخه قبلی

- [DELIVERY-v35.md](DELIVERY-v35.md)

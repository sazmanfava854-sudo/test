# تحویل — RayvarzResend **v38** (تهاتر ۱۵۸ — parity VB Tahator)

Tag: `rayvarzresend-tahator-dtl-v38`

`GET /api/config` → `releaseVersion: 38`

## دریافت

[دانلود RayvarzResend-38.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-38.zip)

## تغییرات نسبت به v37

- **تهاتر درآمدی (۱۵۸)**: `IncmNo` همان **`CI_IncomeCalculation`** است (مثلاً 1025، 1275، 1267، 1288، 100116، **1271**) — مطابق تابع VB `Tahator()`
- حذف نگاشت اشتباه `IncomeCode` → 1270/1278 و ادغام ردیف‌ها برای تهاتر
- بارگذاری همیشه از **`Income_Calculation`** + oddment + مقیاس به `Payable` (مثل VB، نه فقط Accounting_DocDetails)

## نمونه

فیش `050833533237` — ۶ ردیف، جمع `Val` = `Payable` = 2,350,000,000

## نسخه قبلی

- [DELIVERY-v37.md](DELIVERY-v37.md)

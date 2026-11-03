# تحویل — RayvarzResend **v55**

Tag: `rayvarzresend-tahator-dtl-v55` — `releaseVersion: 55`

**[دانلود RayvarzResend-55.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-55.zip)**

## تغییرات نسبت به v54

### تب «تغییر وضعیت چک به خزانه»
- گرید واحد پس از پیش‌نمایش (تکی + اکسل): انتخاب با **کد پیگیری**، رنگ گروه **شماره سند**، انتخاب همه.
- جستجو با شماره سند → نمایش **همه** فیش‌های همان سند در گرید.
- اعتبارسنجی یکپارچه: یکتایی کد پیگیری، هشدار یک‌جا وقتی برای یک سند بیش از یک فیش انتخاب شده.
- اعمال فقط با `selectedTrackingNos` (UPDATE روی کد پیگیری).

شامل v54 (toast ارسال رایورز، برچسب وضعیت فیش، v53 ابطال) و قبل.

## استقرار

`GET /api/config` → `releaseVersion: 55`

یک‌بار **Ctrl+F5** (`app.js?v=112`, `style.css?v=53`).

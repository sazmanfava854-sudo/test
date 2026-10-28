# تحویل — RayvarzResend **v51**

Tag: `rayvarzresend-tahator-dtl-v51` — `releaseVersion: 51`

**[دانلود RayvarzResend-51.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-51.zip)**

## تغییرات نسبت به v50

### ارسال جمعی از اکسل / گرید ارسال‌نشده‌ها
1. **علت خطای 504:** همهٔ فیش‌های انتخاب‌شده در **یک** درخواست HTTP ارسال می‌شدند. هر فиш `SendDelayMs` (پیش‌فرض ۲ ثانیه) + زمان SOAP رایورز دارد؛ بیش از ~۵ فیش معمولاً از timeout درگاه (IIS/nginx، حدود ۶۰ ثانیه) طولانی‌تر می‌شود. سرور کار را تمام می‌کند ولی مرورگر پاسخ HTML «504 Gateway Time-out» می‌گیرد.
2. **رفع:** مرورگر ارسال را به **دسته‌های کوچک** (پیش‌فرض **۴ فیش** در هر درخواست) تقسیم می‌کند، پیشرفت «دسته X از Y» نشان می‌دهد و نتایج را یک‌جا جمع می‌کند.
3. تنظیم اختیاری در `appsettings.json`: `"Rayvarz": { "UnsentBatchChunkSize": 4 }` (۱ تا ۲۰). در `GET /api/config` فیلد `unsentBatch.chunkSize` دیده می‌شود.

شامل **v50** (SSO، تمپلیت اکسل) و نسخه‌های قبل.

## استقرار

1. Zip → `RayvarzResend\` — `appsettings.json` قبلی را نگه دارید — `start.bat`
2. `GET /api/config` → `releaseVersion: 51`
3. یک‌بار Ctrl+F5 (`app.js?v=110`).

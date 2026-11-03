# تحویل — RayvarzResend **v57**

Tag: `rayvarzresend-tahator-dtl-v57` — `releaseVersion: 57`

**[دانلود RayvarzResend-57.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-57.zip)**

## تغییرات نسبت به v56

### گرید تب «تغییر وضعیت چک به خزانه»
- شماره ردیف گرید پیوسته است (۱، ۲، ۳، …) — دیگر شماره ردیف اکسل تکراری نمی‌شود.
- فقط فیش‌هایی که **شماره سند مشترک** دارند رنگ گروه یکسان می‌گیرند؛ ردیف‌های با شماره سند یکتا بدون رنگ (یکسان).
- فیش دیگرِ همان شماره سند اگر مبلغ/تاریخ اکسل با آن نخواند، باز هم **قابل انتخاب** است (اگر کد پیگیری دارد).
- ردیف بدون `TrackingNo` در دیتابیس: «بدون کد پیگیری — قابل انتخاب/ارسال نیست».

### ابطال فیش (FicheSchema)
پیش‌فرض مطابق Sara: درآمد فقط `EumFicheStatus=4`، Duty `EumDutyFicheStatus=2` یا `CanceledDate`.

شامل v56 و قبل.

`GET /api/config` → `releaseVersion: 57` — یک‌بار **Ctrl+F5** (`app.js?v=113`).

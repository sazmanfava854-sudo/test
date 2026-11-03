# تحویل — RayvarzResend **v54**

Tag: `rayvarzresend-tahator-dtl-v54` — `releaseVersion: 54`

**[دانلود RayvarzResend-54.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-54.zip)**

## تغییرات نسبت به v53

### پیام بالای صفحه هنگام ارسال به رایورز
تا **پایان** ارسال (تکی، تهاتر، یا دسته‌ای ارسال‌نشده‌ها)، پیام «در حال ارسال…» **محو نمی‌شود** و با انیمیشن ملایم «در حال پردازش» نمایش داده می‌شود. پس از اتمام، پیام موفقیت/خطا/هشدار جایگزین می‌شود (مثل قبل).

### برچسب‌های رسمی وضعیت فیش
**درآمد (`Income_Fiche.EumFicheStatus`):**
- 0 صدور موقتی — 1 صدور قطعی — 2 چاپ فیش — 3 تائید — 4 ابطال — 5 تائید بانک

**نوسازی/صنفی (`Duty_Fiche.EumDutyFicheStatus`):**
- 0 صدور — 1 تائید — 2 ابطال — 3 صدور موقت — 4 تائید بانک

در `GET /api/config` → `ficheDateChange.statusLabels` و `ficheDateChange.dutyStatusLabels`.

شامل v53 (ابطال Duty و درآمد) و قبل.

## استقرار

`GET /api/config` → `releaseVersion: 54`

یک‌بار **Ctrl+F5** (`app.js?v=111`, `style.css?v=53`).

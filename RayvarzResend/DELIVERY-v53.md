# تحویل — RayvarzResend **v53**

Tag: `rayvarzresend-tahator-dtl-v53` — `releaseVersion: 53`

**[دانلود RayvarzResend-53.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-53.zip)**

## تغییرات نسبت به v52

### ابطال نوسازی/صنفی (Duty_Fiche)
وضعیت‌های رسمی `EumDutyFicheStatus`:
- 0 صدور — 1 تائید — **2 ابطال** — 3 صدور موقت — 4 تائید بانک

**ارسال مجاز نیست** اگر:
- `EumDutyFicheStatus = 2` **یا**
- `CanceledDate` پر باشد.

درآمد شهرسازی: `EumFicheStatus = 4` (ابطال) یا `CanceledDate` — مثل قبل.

شامل v52 و قبل.

## استقرار

`GET /api/config` → `releaseVersion: 53`

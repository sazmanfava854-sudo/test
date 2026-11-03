# تحویل — RayvarzResend **v56**

Tag: `rayvarzresend-tahator-dtl-v56` — `releaseVersion: 56`

**[دانلود RayvarzResend-56.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-56.zip)**

## رفع خطا — ورود اکسل تب رایورز

`Invalid column name 'CanceledDate'` وقتی ستون تاریخ ابطال روی **Income_Fiche** (یا Duty) در Sara وجود ندارد.

### تنظیم `appsettings.json`

```json
"FicheSchema": {
  "IncomeFicheCanceledDateColumn": "",
  "DutyFicheCanceledDateColumn": "CanceledDate"
}
```

- **خالی** = فقط `EumFicheStatus=4` / `EumDutyFicheStatus=2` (بدون SQL روی تاریخ).
- اگر نام ستون در Duty مثلاً `CancelDate` است، همان را بنویسید.
- اگر Duty هم ستون ندارد، هر دو را `""` بگذارید.

شامل v55 و قبل.

`GET /api/config` → `releaseVersion: 56`

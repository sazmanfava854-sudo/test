# تحویل — RayvarzResend **v52**

Tag: `rayvarzresend-tahator-dtl-v52` — `releaseVersion: 52`

**[دانلود RayvarzResend-52.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-52.zip)**

## تغییرات نسبت به v51

### ابطال فیش (CanceledDate)
1. تشخیص ابطال فقط وقتی ستون **`CanceledDate`** در Sara پر باشد (دیگر صرفاً `EumDutyFicheStatus=2` یا `EumFicheStatus=4`).
2. پیام اکسل/جمعی: **«ابطال شده»** (به‌جای «لغو شده»).
3. **ارسال تکی** (درآمد/نوسازی/تهاتر) و **ارسال جمعی** همان شرط را دارند: اگر `CanceledDate` خالی نباشد → «فیش ابطال شده است — ارسال مجاز نیست».

شامل v51 و قبل.

## استقرار

1. Zip → `RayvarzResend\` — `appsettings.json` قبلی — `start.bat`
2. `GET /api/config` → `releaseVersion: 52`

# تحویل — RayvarzResend **v43**

Tag: `rayvarzresend-tahator-dtl-v43` — `releaseVersion: 43`

**[دانلود RayvarzResend-43.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-43.zip)**

## تغییرات نسبت به v42

- **رفع خطای تب «تغییر تاریخ چک»:** پس از جستجو با کد پیگیری (`TrackingNo`) خطای  
  `Unable to cast object of type 'System.Guid' to type 'System.IConvertible'`  
  برطرف شد — `NidInstallment` در `Installment_List` به‌صورت **uniqueidentifier (Guid)** خوانده می‌شود.

شامل همه قابلیت‌های v42 (تاریخ چک، مجوز مدیریت، رمز، و v41 eligibility).

## استقرار

1. Zip را باز کنید → `RayvarzResend\`
2. `appsettings.json` را تنظیم کنید.
3. `start.bat` یا `RayvarzResend.Web.exe`
4. `GET /api/config` → `releaseVersion: 43`

## اتصال به سیستم MIS (HR خارجی)

### ۱. تنظیم رمز عبور (امن)

در PowerShell داخل پوشه API:

```powershell
cd src\HRPerformance.API
dotnet user-secrets init
dotnet user-secrets set "HrIntegration:Password" "رمز-واقعی-ITCMISUserReader"
```

یا در `appsettings.json` (فقط برای تست محلی):

```json
"HrIntegration": {
  "Server": "mdc-aogmain2016\\sql2016",
  "Database": "MIS",
  "UserId": "ITCMISUserReader",
  "Password": "رمز-شما",
  "Enabled": true,
  "SourceType": "SQLView",
  "SyncDaysBack": 30
}
```

### ۲. اجرای اسکریپت‌های دیتابیس

```sql
-- بعد از 08_SeedData.sql
09_Migration_AttendanceExternalId.sql
10_HrMisIntegrationSeed.sql
12_Migration_HrMisSyncState.sql
```

### ۳. View منبع داده

`MIS.dbo.HZG_View_HourlyLeave`

فیلتر پیش‌فرض: `ProvinceCode = '147'` (قابل تغییر در `HrIntegration:ProvinceCode`)

| فیلد MIS | کاربرد در سیستم |
|----------|-----------------|
| PerCod | کد پرسنلی → Employees.PersonnelCode |
| Name / LastName | نام کارمند (سینک خودکار پرسنل) |
| NationalIDNo | کد ملی |
| StartDate / StartTime / EndTime | زمان مرخصی ساعتی |
| LeaveDurationMinutes | امتیازدهی Rule Engine |
| ID | ExternalId (کلید یکتا) |

### ۴. جریان سینک

```
MIS View → MisHrDataReader
         → MisHrEmployeeSyncService (ایجاد/بروزرسانی کارمند)
         → AttendanceLogs
         → RuleEngine (امتیاز مرخصی ساعتی)
```

### ۵. سینک دستی (API)

```
POST /api/attendancesync/run
GET  /api/attendancesync/status
POST /api/attendancesync/run-month?shamsiYear=1404&shamsiMonth=1
Authorization: Bearer {admin-token}
```

### ۵.۱ تنظیمات سینک از داخل برنامه (پیشنهادی)

بعد از لاگین با نقش مدیر:
**تنظیمات > سینک MIS**

| تنظیم | توضیح |
|-------|--------|
| سینک خودکار پس‌زمینه | پیش‌فرض **خاموش** — تا زمانی که فعال نکنید، هنگام اجرا داده لود نمی‌شود |
| محدودیت تعداد پرسنل | برای تست مقدار `10` بگذارید |
| تعداد ماه در هر اجرا | پیش‌فرض `1` (هر بار فقط یک ماه) |
| سینک دستی | دکمه «سینک دستی» در همان صفحه |

در `appsettings` فقط **اتصال MIS** (Server, UserId, Password) باقی می‌ماند.

API:
```
GET  /api/hrintegration/settings
PUT  /api/hrintegration/settings
POST /api/attendancesync/run
```

اسکریپت دیتابیس: `13_Migration_HrIntegrationSettings.sql`

### ۵.۲ سینک ماهانه (پیش‌فرض — سریع‌تر)

برای جلوگیری از کندی کوئری MIS، حالت پیش‌فرض `SyncMode: Monthly` است:

- سامانه **بلافاصله** بالا می‌آید
- اولین سینک **۱۵ ثانیه** بعد از استارت (قابل تنظیم)
- هر اجرا فقط **یک ماه** از MIS خوانده می‌شود
- ماه‌های قبلی به‌تدریج در اجراهای بعدی (هر ۵ دقیقه) پر می‌شوند

```json
"HrIntegration": {
  "SyncMode": "Monthly",
  "InitialSyncMonthsBack": 12,
  "MonthsPerSyncRun": 1,
  "SyncStartupDelaySeconds": 15,
  "ShamsiYearPrefix": "1404"
}
```

حالت‌های دیگر:
- `DaysBack` — مثل قبل با `SyncDaysBack` (پیش‌فرض ۳۰ روز)
- `DateRange` — با `SyncFromDate` و `SyncToDate`

بعد از نصب، اسکریپت `12_Migration_HrMisSyncState.sql` را اجرا کنید.

### ۵.۲ تست محلی — بدون سینک خودکار

**توجه:** داده‌ها هنگام `Building...` لود نمی‌شوند. بعد از `dotnet run` سرویس پس‌زمینه شروع می‌کند.

برای تست با ۱۰ نفر و بدون سینک خودکار، در `appsettings.Development.json`:

```json
"HrIntegration": {
  "Enabled": true,
  "BackgroundSyncEnabled": false,
  "EmployeeLimit": 10,
  "InitialSyncMonthsBack": 1,
  "MonthsPerSyncRun": 1
}
```

- `BackgroundSyncEnabled: false` → هیچ داده‌ای خودکار لود نمی‌شود
- `EmployeeLimit: 10` → فقط ۱۰ پرسنل اول MIS
- سینک دستی فقط وقتی بخواهید: `POST /api/attendancesync/run`

اگر لاگ `ranges=6` دیدید یعنی `MonthsPerSyncRun` روی ۶ است — برای تست روی `1` بگذارید.

### ۶. عیب‌یابی (وقتی کارمندان ۰ است)

```
GET /api/attendancesync/diagnostic
GET /api/attendancesync/diagnostic?shamsiYear=1404&shamsiMonth=1
Authorization: Bearer {admin-token}
```

خروجی تعداد رکوردها را در هر مرحله فیلتر نشان می‌دهد:
- کل View
- بعد از `SyncDaysBack`
- بعد از `ProvinceCode`
- بعد از سال شمسی
- تعداد کارمندان ثبت‌شده در HR

تنظیمات قابل تغییر در `appsettings.json`:

```json
"HrIntegration": {
  "ApplyProvinceFilter": true,
  "ApplyShamsiYearFilter": true,
  "ProvinceCode": "147",
  "ShamsiYearPrefix": "1404",
  "SyncMode": "Monthly",
  "InitialSyncMonthsBack": 12
}
```

برای سینک کل سال یکجا (کند): `"SyncMode": "DaysBack", "SyncDaysBack": 365`

اگر MIS داده دارد ولی فیلترها ۰ برمی‌گردانند، موقتاً `ApplyShamsiYearFilter: false` بگذارید.

سینک خودکار هر ۵ دقیقه توسط Background Service.

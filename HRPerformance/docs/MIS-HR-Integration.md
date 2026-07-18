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
Authorization: Bearer {admin-token}
```

### ۶. عیب‌یابی (وقتی کارمندان ۰ است)

```
GET /api/attendancesync/diagnostic
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
  "SyncDaysBack": 365
}
```

اگر MIS داده دارد ولی فیلترها ۰ برمی‌گردانند، موقتاً `ApplyShamsiYearFilter: false` بگذارید.

سینک خودکار هر ۵ دقیقه توسط Background Service.

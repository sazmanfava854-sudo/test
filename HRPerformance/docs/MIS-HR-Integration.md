## اتصال به سیستم MIS (HR خارجی)

### ۱. تنظیم اتصال (فقط مدیر فنی سرور)

در `appsettings.json` فقط اطلاعات اتصال MIS:

```json
"HrIntegration": {
  "Enabled": true,
  "Server": "mdc-aogmain2016\\sql2016",
  "Database": "MIS",
  "UserId": "ITCMISUserReader",
  "Password": "رمز-شما",
  "SourceType": "SQLView"
}
```

### ۲. دریافت داده توسط کاربر (بعد از لاگین)

هیچ داده‌ای به‌صورت خودکار لود نمی‌شود.

1. با نقش مدیر وارد شوید
2. بروید به **تنظیمات > دریافت از MIS**
3. **بازه تاریخ** (از / تا) را انتخاب کنید
4. دکمه **دریافت داده از MIS** را بزنید

API:
```
POST /api/attendancesync/run-range
Authorization: Bearer {token}

{
  "fromDate": "2025-01-01",
  "toDate": "2025-01-31",
  "provinceCode": "147",
  "shamsiYearPrefix": "1404",
  "employeeLimit": 10
}
```

### ۳. اسکریپت‌های دیتابیس

```
09_Migration_AttendanceExternalId.sql
10_HrMisIntegrationSeed.sql
12_Migration_HrMisSyncState.sql  (اختیاری — نسخه‌های قدیمی)
```

### ۴. View منبع داده

`MIS.dbo.HZG_View_HourlyLeave`

| فیلد MIS | کاربرد |
|----------|--------|
| PerCod | کد پرسنلی |
| Name / LastName | نام کارمند |
| StartDate | بازه فیلتر تاریخ |
| ID | ExternalId |

### ۵. عیب‌یابی

```
GET /api/attendancesync/diagnostic?fromDate=2025-01-01&toDate=2025-01-31
```

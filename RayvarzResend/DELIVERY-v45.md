# تحویل — RayvarzResend **v45**

Tag: `rayvarzresend-tahator-dtl-v45` — `releaseVersion: 45`

**[دانلود RayvarzResend-45.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-45.zip)**

## تغییرات نسبت به v44

- **تب شاهکار — اتصال `security10`:** پشتیبانی از `Shahkar:SqlServer` و `Shahkar:SqlInstance` (بدون نیاز به `\\` در JSON)، اصلاح نام instance چسبیده، `Encrypt=False` برای SQL 2012، راهنمای خطا در UI، `GET /api/shahkar/connection-test`.

شامل v44 (UI تاریخ چک + Guid fix) و نسخه‌های قبل.

## پیکربندی شاهکار (نمونه)

در `appsettings.json`:

```json
"Shahkar": {
  "SqlServer": "172.16.8.138",
  "SqlInstance": "sql2012"
},
"ConnectionStrings": {
  "Security": "Database=security10;User Id=hService;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;"
}
```

رمز واقعی `hService` را جایگزین `YOUR_PASSWORD` کنید. اگر رمز `;` یا `\` دارد در JSON escape کنید.

## استقرار

1. Zip → `RayvarzResend\` — `appsettings.json` — `start.bat`
2. `GET /api/config` → `releaseVersion: 45` و `shahkar.dataSource` باید `172.16.8.138\sql2012` باشد.
3. با دسترسی شاهکار: `GET /api/shahkar/connection-test` → `ok: true`

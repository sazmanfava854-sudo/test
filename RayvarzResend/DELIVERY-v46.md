# تحویل — RayvarzResend **v46**

Tag: `rayvarzresend-tahator-dtl-v46` — `releaseVersion: 46`

**[دانلود RayvarzResend-46.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-46.zip)**

## تغییرات نسبت به v45

خطای `Cannot open database "security10" requested by the login` یعنی سرور SQL **رمز را قبول کرده** و فقط باز کردن پایگاه `security10` رد شده است (خطای ۴۰۶۰). v46 در تب شاهکار، با همان کاربر به `master` وصل می‌شود و علت را فارسی نشان می‌دهد:

- پایگاه روی این instance نیست → پورت TCP را در `Shahkar:SqlPort` بگذارید (وقتی UDP 1434 / SQL Browser از سرور برنامه بسته است).
- پایگاه هست ولی `hService` داخل آن کاربر ندارد → اسکریپت `CREATE USER` / `ALTER USER` برای اجرا با login مدیر.
- تفاوت حروف نام پایگاه → اتصال دوباره با نام دقیق.
- رمز دارای `;` → فیلد جدا `Shahkar:Password` تا رشته اتصال قطع نشود.

شامل v45 و v44.

## پیکربندی

در `appsettings.json` کنار `start.bat` (بعد از کپی zip، رمز واقعی را بگذارید و برنامه را یک‌بار ببندید و دوباره اجرا کنید):

```json
"Shahkar": {
  "SqlServer": "172.16.8.138",
  "SqlInstance": "sql2012",
  "SqlPort": 0,
  "Database": "security10",
  "UserId": "hService",
  "Password": "رمز_واقعی_hService"
}
```

اگر `SqlPort` را از SSMS گرفتید (عدد غیر صفر)، همان پورت استفاده می‌شود و نام instance دور زده می‌شود:

```sql
SELECT local_tcp_port FROM sys.dm_exec_connections WHERE session_id = @@SPID;
```

## استقرار

1. Zip → `RayvarzResend\` — `appsettings.json` — `start.bat`
2. تب شاهکار را باز کنید. بالای فرم، سرور مؤثر، کاربر و طول رمز (نه خود رمز) دیده می‌شود.
3. `GET /api/config` → `releaseVersion: 46`

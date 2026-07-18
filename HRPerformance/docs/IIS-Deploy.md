# استقرار روی IIS (Windows Server / Windows 10+)

## پیش‌نیازها

| نرم‌افزار | توضیح |
|-----------|--------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | برای `publish` روی سرور یا PC توسعه |
| [ASP.NET Core 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) | **الزامی روی سرور IIS** — شامل Runtime + ASP.NET Core Module |
| IIS | نقش **Web Server (IIS)** + **ASP.NET 4.8** (اختیاری) |
| SQL Server | برای محیط واقعی (اسکریپت‌های `database/01` تا `10`) |

بعد از نصب Hosting Bundle حتماً **IIS را یک‌بار Restart** کنید:

```cmd
iisreset
```

---

## مرحله ۱ — Publish

در پوشه پروژه:

```powershell
.\publish-iis.ps1
```

خروجی در: `HRPerformance\publish\iis\`

یا دستی:

```powershell
dotnet publish src\HRPerformance.API\HRPerformance.API.csproj -c Release -o C:\inetpub\HRPerformance
```

---

## مرحله ۲ — تنظیم appsettings

فایل `publish\iis\appsettings.Production.json` را ویرایش کنید:

- `ConnectionStrings:DefaultConnection` → SQL Server واقعی
- `HrIntegration:Password` → رمز MIS
- `Jwt:Key` → کلید امن (حداقل ۳۲ کاراکتر)
- `Cors:Origins` → آدرس سایت IIS

**نکته:** در IIS مقدار `Urls` در `appsettings.json` را حذف کنید یا خالی بگذارید؛ پورت را IIS تعیین می‌کند.

---

## مرحله ۳ — IIS Manager

### Application Pool

1. **Add Application Pool**
   - Name: `HRPerformance`
   - **.NET CLR version:** `No Managed Code`
   - Managed pipeline: `Integrated`

2. **Advanced Settings** (اختیاری)
   - Identity: `ApplicationPoolIdentity` یا حساب سرویس با دسترسی SQL

### Site

1. **Add Website** (یا Application زیر سایت موجود)
   - Physical path: `C:\...\HRPerformance\publish\iis`
   - Binding: مثلاً `http` port `80` یا `https` port `443`
   - Application pool: `HRPerformance`

2. **Environment Variable** (Site → Configuration Editor یا `web.config`):

```xml
<aspNetCore processPath="dotnet"
            arguments=".\HRPerformance.API.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

فایل `web.config` معمولاً بعد از `dotnet publish` خودکار ساخته می‌شود.

---

## حالت Demo روی IIS (بدون SQL Server)

در `web.config` یا Variables سایت:

```xml
<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Demo" />
```

یا در Application Pool → Environment Variables: `ASPNETCORE_ENVIRONMENT=Demo`

ورود: `admin` / `Admin@123`

---

## دسترسی پوشه‌ها

به حساب App Pool (مثلاً `IIS AppPool\HRPerformance`) بدهید:

- **Modify** روی `publish\iis\logs`
- **Modify** روی `publish\iis\uploads`

---

## دیتابیس

1. در SSMS اسکریپت‌ها را اجرا کنید: `database\01` تا `10`
2. Connection String را در `appsettings.Production.json` تنظیم کنید
3. اگر از `Trusted_Connection` استفاده می‌کنید، Identity مربوط به App Pool باید به SQL دسترسی داشته باشد

---

## تست

| آدرس | توضیح |
|------|--------|
| `http://SERVER/` | فرانت‌اند (wwwroot) |
| `http://SERVER/swagger` | API (در Development؛ در Production Swagger خاموش است مگر `ASPNETCORE_ENVIRONMENT=Development`) |
| `http://SERVER/api/health` | سلامت سرویس و دیتابیس |

سینک MIS دستی (با توکن admin):

```
POST http://SERVER/api/attendancesync/run
```

---

## خطاهای رایج

| خطا | راه‌حل |
|-----|--------|
| HTTP 500.30 | Hosting Bundle نصب نیست یا IIS restart نشده |
| HTTP 500.31 | .NET 8 Runtime نصب نیست |
| خطای SQL | Connection string یا دسترسی App Pool به SQL |
| صفحه سفید | `logs\stdout_*.log` و `logs\hr-performance-*.log` را ببینید |
| پورت اشغال | در IIS binding پورت دیگری انتخاب کنید (مثلاً 8080) |

---

## Publish از ZIP دمو

1. ZIP را Extract کنید
2. `publish-iis.bat` را اجرا کنید
3. مسیر `publish\iis` را در IIS به‌عنوان Physical Path قرار دهید

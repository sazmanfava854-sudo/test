# تحویل — RayvarzResend **نسخه آخر** (v26)

شاخه: `cursor/unified-excel-epay-release-ffcb` — اکسل دسته‌ای (قبض/پرداخت بدون ستون نوع)، جستجوی mixed Income سپس Duty، دروازه epay قبل از ارسال، به‌همراه Accounting_Doc و تهاتر v25.

## دریافت

| روش | مسیر |
|-----|------|
| **Tag نسخه** | `rayvarzresend-unified-excel-epay-v26` |
| Zip publish | `RayvarzResend-26.zip` |
| Zip سورس | `RayvarzResend-source.zip` |
| شاخه | [cursor/unified-excel-epay-release-ffcb](https://github.com/sazmanfava854-sudo/test/tree/cursor/unified-excel-epay-release-ffcb) |

### لینک مستقیم Zip (آخرین — شاخه v26 + SSO ریشه سایت)

[دانلود RayvarzResend-26.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/unified-excel-epay-release-ffcb/RayvarzResend-26.zip)

Tag ثابت (ممکن است از شاخه عقب‌تر باشد):

[RayvarzResend-26.zip روی tag v26](https://github.com/sazmanfava854-sudo/test/raw/rayvarzresend-unified-excel-epay-v26/RayvarzResend-26.zip)

### اجرا روی سرور ویندوز

Zip فقط **یک پوشه** دارد: `RayvarzResend\`

| فایل | کاربرد |
|------|--------|
| `RayvarzResend\start.bat` | اجرا (پورت 5088) |
| `RayvarzResend\RayvarzResend.Web.exe` | exe (نیاز به .NET 8 Runtime روی سرور) |
| `RayvarzResend\appsettings.json` | **تنها** فایل تنظیمات |

`appsettings.Production.json` خوانده نمی‌شود — اگر از قبل کنار exe هست حذف کنید.

`Rayvarz:DryRun=false` و `AccountingDoc:DryRun=false` داخل همان `appsettings.json`.

پس از اجرا: `GET /api/config` → `releaseVersion: 26` ، `accountingDoc.dryRun: false`.

### SSO (سند شناسه شهروندی — login.mashhad.ir)

**قبل از تنظیم `appsettings`:** اعتبار را **خارج از برنامه** با اسکریپت سند (صفحه ۲۰–۲۶) تست کنید:

- ویندوز (کنار Zip، PowerShell): بدون `<>` — مثال: `.\test-mashhad-sso-loginkey.ps1 -ApiName "myApiUser" -ClientId "53db..." -SecretKey "fullSecret"` (یک خط یا با backtick انتهای هر خط)
- لینوکس: `bash scripts/test-mashhad-sso-loginkey.sh "<apiName>" "<ClientId>" "<SecretKey>"`

هدرها: `apiName` = **نام کاربری کاربردی برنامه** (از مدیر SSO)، `requestTime` از `getCurrentTime`، `apiSecret` = `SHA256(SecretKey + requestTime)` hex. بدنه: `Time`, `Hash` (همان مقدار), `ClientId`, `State`, `UserType`, `DomainID`. نام نمایشی برنامه (مثلاً FinancialAssistant) **apiName نیست** مگر همان را در پورتال ثبت کرده باشند.

در `Auth:Shimas`:

**روال:** `loginKey` → `Authentication/Start/{LoginKey}` → callback با `username` + `refresh_token` + `state` → `getAccessToken`.

| کلید | معنی |
|------|------|
| `SsoRegisteredReturnUrl` | **همان «برگشت آدرس»** که به برنامه‌نویس SSO می‌دهید — `https://city.mashhad.ir:5065/management` |
| `PublicBaseUrl` | `https://city.mashhad.ir:5065` (بدون مسیر) |
| `PostLoginDefaultPath` | `/management/` — بعد از SSO کاربر به **صفحهٔ مدیریت** می‌رود؛ فقط فرم‌های مجاز (`/api/auth/me`) |
| `ApplicationPath` | اگر IIS زیرمسیر دارد مثل `/RayvarzResend` — برگشت آدرس = `https://city.mashhad.ir:5065/RayvarzResend` |
| چک | `GET /api/auth/sso-return-url` → فیلد `registerInSsoPortal`؛ `GET /api/auth/sso-loginkey-check` یا `/auth/login?debug=1` → آیا SSO برای این ClientId/Secret `loginKey` می‌دهد (403 = Client info mismatch) |
| 403 `Client info missmatched` | اول `test-mashhad-sso-loginkey.ps1`؛ بعد `GET /api/auth/sso-loginkey-probe` و `&apiName=<نام کاربری پورتال>` بدون تغییر appsettings |
| `CallbackPath` | `/management` = بازگشت SSO روی صفحهٔ مدیریت (همان ReturnUrl پورتال) |
| `ApiName` / `SSOUserName` | **نام کاربری کاربردی برنامه** — هدر `apiName` (سند ص ۲۰) |
| `ClientId` / `ClientSecret` | **شناسه** و **SecretKey** همان کاربردی در پورتال |
| `ApiBaseUrl` | `https://login.mashhad.ir` |
| `IncludeReturnUrlInLoginKey` | **`false`** — برگشت فقط از آدرس ثبت‌شده در پورتال |
| `AllowLegacyLoginUrlWithoutLoginKey` | **`false`** — fallback به Login.aspx معمولاً → `Profile.aspx` |
| `ClientId` / `ClientSecret` | ثبت SSO + هش `SHA256(Secret+requestTime)` |
| `UseLoginKeyOnRedirect` | **`true`** — `loginKey` سپس `Authentication/Start/{loginKey}` (سند SSO ص ۲۶) |
| گیر روی `Profile.aspx` | ورود **بدون** `ReturnUrl` (لینک مستقیم با `lkey` یا ClientId ناهماهنگ). همیشه از `https://city.mashhad.ir:5065` → `/auth/login` شروع کنید؛ در پورتال SSO **برگشت آدرس** = همان `PublicBaseUrl` |
| `ClientId` / `ClientSecret` / `ApiName` | باید با ثبت SSO (جدول ۱) یکی باشد — `lkey` در URL باید همان `ClientId` باشد |
| `LoginStartUrlTemplate` | `https://login.mashhad.ir/Authentication/Start/{loginKey}` |
| `LoginState` | فقط برای loginKey؛ در Login.aspx قدیمی معمولاً `state` نمی‌آید — خالی بگذارید یا همان پیش‌فرض (بازگشت بدون state مجاز است) |
| `AutoProvisionUsers` | `false` = کاربر باید از قبل در «مدیریت کاربران» با کد ملی/دامین ثبت شده باشد؛ بعد از SSO بدون رکورد → بازگشت به login با پیام خطا |

403 **Client info missmatched** = طبق سند (صفحه ۲۱): هدر `apiName` / `ClientId` / `apiSecret` با ثبت کاربردی برنامه یکی نیست.

بعد از OK شدن اسکریپت، در `Auth:Shimas` بگذارید: `ApiName`/`SSOUserName` = همان apiName، `ClientId`، `ClientSecret` = SecretKey کامل.

بررسی: `GET /api/auth/mode` → `signingApiName`, `clientIdHint`, `clientSecretLooksShort`.

ساخت Zip: `bash RayvarzResend/scripts/build-release-zip.sh`

`GET /api/config` → `releaseVersion: 26` ، `releaseLabel: نسخه آخر`

## خلاصه تغییرات نسبت به v25

| موضوع |
|--------|
| تب ارسال دسته‌ای: ورود از اکسل (۲ ستون قبض/پرداخت)، تمپلیت، کتابخانه xlsx محلی |
| جستجو: ابتدا `Income_Fiche`، سپس `Duty_Fiche`؛ تعارض هر دو → لاگ، بدون افزودن به گرید |
| قبل از ارسال تک/دسته‌ای: بررسی حضور فیش در epay (`EpayFichePresenceChecker`) |
| بهینه‌سازی SQL batch lookup (کاهش timeout اکسل) |
| UI گرید دسته‌ای، وضعیت «فایل در حال بررسی است…»، لاگ نتیجه ارسال |
| ورود: کاربران سازمانی SSO از `https://city.mashhad.ir:5065` → `/management/` |
| ورود ادمین **بدون SSO**: `https://city.mashhad.ir:5065/login.html` یا داخلی `http://5.252.216.140:8070/login.html` (`AllowAdminLocalLoginOnPublicHost` + `PreferLocalLoginHosts`) |
| ورود HTTP داخلی | کوکی نشست `SameAsRequest` است (بدون Secure اجباری) — وگرنه بعد از POST ورود، `/api/auth/me` 401 می‌شود |
| bootstrap اول | نام کاربری `admin` یا کد ملی ثبت‌شده در `BootstrapAdmin` — رمز از `Auth:BootstrapAdmin:Password` |
| ادمین حذف/ناشناخته | اگر کاربر `BootstrapAdmin:Username` در جدول نباشد، در راه‌اندازی دوباره ساخته می‌شود (حتی اگر کاربران دیگر باشند) |
| رمز ادمین فراموش شد | `"ResetPasswordOnStartup": true` در `BootstrapAdmin` → ری‌استارت → ورود با رمز appsettings → دوباره **`false`** کنید |
| چند دامین برای یک کاربر | در همان ستون `AppUser.Domain` با کاما: `hoseine-sh,sadathoseini-sh` — ورود SSO با هر کدام به همان کاربر می‌رسد (بدون جدول جدید) |
| خروج | به `/login.html` می‌رود (نه `/auth/login`) تا نشست SSO کاربر را فوراً دوباره وارد نکند |
| پس از ورود: صفحهٔ مدیریت `/management/` با کارت فرم‌های مجاز؛ لینک هر کارت → تب مربوط |

## تست

```bash
cd RayvarzResend && dotnet test   # 462
```

## نسخه قبلی

- [DELIVERY-v25.md](DELIVERY-v25.md) — tag `rayvarzresend-noskhe-akhar`

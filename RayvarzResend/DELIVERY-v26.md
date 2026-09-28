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

### SSO (مثل RuleEngine / login.mashhad.ir)

در `Auth:Shimas` (سند شناسه شهروندی):

**روال:** `loginKey` → `Authentication/Start/{LoginKey}` → callback با `username` + `refresh_token` + `state` → `getAccessToken`.

| کلید | معنی |
|------|------|
| `SsoRegisteredReturnUrl` | **همان «برگشت آدرس»** که به برنامه‌نویس SSO می‌دهید — `https://city.mashhad.ir:5065/MANAGMENT` |
| `PublicBaseUrl` | `https://city.mashhad.ir:5065` (بدون مسیر) |
| `PostLoginDefaultPath` | `/MANAGMENT` — بعد از SSO کاربر به **صفحهٔ مدیریت** می‌رود؛ فقط فرم‌های مجاز (`/api/auth/me`) |
| `ApplicationPath` | اگر IIS زیرمسیر دارد مثل `/RayvarzResend` — برگشت آدرس = `https://city.mashhad.ir:5065/RayvarzResend` |
| چک | `GET /api/auth/sso-return-url` → فیلد `registerInSsoPortal` |
| `CallbackPath` | `/MANAGMENT` = بازگشت SSO روی صفحهٔ مدیریت (همان ReturnUrl پورتال) |
| `ApiName` | همان **SSOUserName** در RuleEngine (مثلاً `zavabetapp`) — هدر `apiName` |
| `ClientId` / `ClientSecret` | همان **SSOClientId** / **SSOSecret** |
| `ApiBaseUrl` | همان **SSOBaseUrl** (`https://login.mashhad.ir`) |
| `IncludeReturnUrlInLoginKey` | **`false`** مثل RuleEngine — برگشت فقط از آدرس ثبت‌شده در پورتال |
| `AllowLegacyLoginUrlWithoutLoginKey` | **`false`** — fallback به Login.aspx معمولاً → `Profile.aspx` |
| `ClientId` / `ClientSecret` | ثبت SSO + هش `SHA256(Secret+requestTime)` |
| `UseLoginKeyOnRedirect` | **`true`** — `loginKey` سپس `Authentication/Start/{loginKey}` (سند SSO ص ۲۶) |
| گیر روی `Profile.aspx` | ورود **بدون** `ReturnUrl` (لینک مستقیم با `lkey` یا ClientId ناهماهنگ). همیشه از `https://city.mashhad.ir:5065` → `/auth/login` شروع کنید؛ در پورتال SSO **برگشت آدرس** = همان `PublicBaseUrl` |
| `ClientId` / `ClientSecret` / `ApiName` | باید با ثبت SSO (جدول ۱) یکی باشد — `lkey` در URL باید همان `ClientId` باشد |
| `LoginStartUrlTemplate` | `https://login.mashhad.ir/Authentication/Start/{loginKey}` |
| `LoginState` | فقط برای loginKey؛ در Login.aspx قدیمی معمولاً `state` نمی‌آید — خالی بگذارید یا همان پیش‌فرض (بازگشت بدون state مجاز است) |
| `AutoProvisionUsers` | `false` = کاربر باید از قبل در «مدیریت کاربران» با کد ملی/دامین ثبت شده باشد؛ بعد از SSO بدون رکورد → بازگشت به login با پیام خطا |

403 **Client info missmatched** = یکی از این‌ها با پورتال SSO (جدول ۱) یکی نیست:
- `SSOUserName` / `ApiName` (هدر **apiName** — مثل `zavabetapp` فقط برای همان سامانه)
- `SSOClientId` / `ClientId`
- `SSOSecret` / `ClientSecret` — **کل SecretKey** (نه `D2fbf` کوتاه)

می‌توانید همان بلوک RuleEngine را در ریشه `appsettings.json` بگذارید:

```json
"Settings": {
  "SSOBaseUrl": "https://login.mashhad.ir",
  "SSOClientId": "<ClientId ثبت دستیار مالی>",
  "SSOSecret": "<Secret کامل>",
  "SSOUserName": "<نام کاربری ثبت سامانه>"
}
```

یا داخل `Auth:Shimas` با کلیدهای `SSOUserName` / `SSOClientId` / `SSOSecret`.

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
| ورود: کاربران سازمانی SSO از `/`؛ ادمین از `https://city.mashhad.ir:5065/login.html` (کد ملی/رمز bootstrap) |
| پس از ورود: اولین تب مطابق دسترسی (رایورز، چک خزانه، …) |

## تست

```bash
cd RayvarzResend && dotnet test   # 462
```

## نسخه قبلی

- [DELIVERY-v25.md](DELIVERY-v25.md) — tag `rayvarzresend-noskhe-akhar`

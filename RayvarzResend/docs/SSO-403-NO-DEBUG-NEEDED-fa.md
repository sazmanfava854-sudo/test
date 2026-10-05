# loginKey 403 — چرا دیباگ نمی‌خورد و چه کار کنید

## چرا وارد دیباگ (breakpoint) نمی‌شوید؟

وقتی این آدرس‌ها را در مرورگر باز می‌کنید:

- `https://city.mashhad.ir:5065/auth/login?debug=1`
- `/api/auth/sso-loginkey-check`
- `/api/auth/sso-loginkey-probe`
- `/api/auth/sso-signing-preview`

کد روی **سرور city (IIS / w3wp)** اجرا می‌شود، نه روی process **F5** در Visual Studio روی لپ‌تاپ.

| روش | نتیجه |
|-----|--------|
| F5 روی `localhost:5088` | breakpoint روی city **نمی‌خورد** |
| Attach به `w3wp.exe` سایت 5065 + refresh همان URL | breakpoint **می‌خورد** |

حتی با Attach، وقتی **همهٔ ۱۲ variant** در `sso-loginkey-probe` با **403 Client info missmatched** شکست می‌خورند، مشکل **فرمول هش یا خط کد نیست** — SSO سه‌تایی **apiName + ClientId + SecretKey** را با ثبت پورتال نمی‌شناسد.

## خروجی شما یعنی چه؟

- `getCurrentTime: OK` → شبکه و `login.mashhad.ir` درست است.
- `loginKeyOk: false`, `403`, `anyOk: false` روی همهٔ فرمول‌ها → **اعتبار اشتباه یا apiName اشتباه** (نه `HashEncoding`).

تا loginKey OK نشود:

- روش ورود = **Login.aspx** (fallback)
- بعد از لاگین = **Profile.aspx** (بدون برگشت به city)
- middleware callback = **QueryString خالی** (هرگز به city نمی‌رسید)

## کارهای عملی (به ترتیب)

### ۱) از پورتال SSO / ادمین بگیرید (جدول کاربردی برنامه)

| فیلد پورتال | کلید appsettings |
|-------------|------------------|
| نام کاربری کاربردی (apiName) | `Auth:Sso:ApiName` یا `SSOUserName` |
| شناسه / ClientId / lkey | `ClientId` / `LKey` |
| SecretKey کامل | `ClientSecret` |

برای دستیار مالی، **apiName** ثبت‌شده در پورتال معمولاً **`financial_Assist`** است (نام نمایشی برنامه ممکن است FinancialAssistant باشد).

### ۲) appsettings روی **سرور** (نه فقط git)

فایل واقعی IIS معمولاً:

`RayvarzResend.Web\appsettings.json` یا `appsettings.Production.json`

بعد از deploy، `ClientSecret` را عوض کنید و **pool را recycle** کنید.

### ۳) مقایسه با RuleEngine (اگر دارید)

`GET /api/auth/sso-credential-compare`

`GET /api/auth/sso-loginkey-probe?profile=settings`

اگر **settings** OK شد → همان سه مقدار را در `Auth:Sso` بگذارید.

### ۴) امتحان apiName دیگر (بدون deploy)

`GET /api/auth/sso-loginkey-probe?apiName=نام_کاربری_از_پورتال`

اگر باز 403 → **ClientId یا Secret** با پورتال یکی نیست (Secret اشتباه رایج‌ترین است).

### ۵) اسکریپت مستقل روی سرور city

```powershell
cd RayvarzResend\scripts
.\test-mashhad-sso-loginkey.ps1 -SecretKey "SECRET_FROM_PORTAL" -ApiName "REAL_API_USER" -ClientId "53db42619cf3C333b13a18D34fbd9111"
```

تا وقتی اسکریپت **loginKey OK** ندهد، اپ هم OK نمی‌شود.

## بعد از OK شدن loginKey

`/auth/login?debug=1` باید بگوید **`Authentication/Start/{loginKey}`** — آن وقت SSO و callback را دوباره تست کنید.

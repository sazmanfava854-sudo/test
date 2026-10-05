# گیر کردن روی Profile.aspx بعد از لاگین مشهد

## علامت

| مرحله | URL |
|--------|-----|
| قبل لاگین | `Login.aspx?lkey=...&returnUrl=https%3A%2F%2Fcity.mashhad.ir%3A5065%2Fmanagement` |
| بعد لاگین | `Profile.aspx?lkey=...` **بدون** ریدایرکت به city |

یعنی **احراز هویت در سامانهٔ مشهد OK** است، اما **برنامهٔ شما (FinancialAssistant / city:5065) اصلاً callback نمی‌گیرد**. به همین دلیل در Visual Studio `QueryString = {}` می‌بینید — درخواست به `city.mashhad.ir:5065/management?refresh_token=...` **هرگز نمی‌رسد**.

## چرا این اتفاق می‌افتد؟

1. **روش قدیمی `Login.aspx`** (fallback وقتی API **loginKey** خطا می‌دهد) با ثبت «برگشت آدرس» در پورتال SSO **همیشه** درست کار نمی‌کند و خیلی وقت‌ها بعد از لاگین کاربر را روی **Profile.aspx** نگه می‌دارد.
2. **«برگشت آدرس» ثبت‌شده در پورتال SSO** با مقداری که در `returnUrl` می‌فرستید **کاراکتر به کاراکتر** یکی نیست (اسلش آخر، `http`/`https`، پورت `:5065`, مسیر `/management`).
3. **`ClientId` / `SecretKey` / `apiName`** با پورتال یکی نیست → loginKey 403 → همیشه `Login.aspx` (همان سناریوی Profile).

روال درست طبق سند SSO:

`loginKey` → `https://login.mashhad.ir/Authentication/Start/{loginKey}` → برگشت به آدرس ثبت‌شده → `?username&refresh_token`.

## کارهایی که باید انجام دهید (به ترتیب)

### ۱) فقط از city شروع کنید

همیشه:

`https://city.mashhad.ir:5065/login.html` یا `https://city.mashhad.ir:5065/auth/login?debug=1`

لینک مستقیم `Login.aspx?lkey=...` **بدون** `returnUrl` یا با lkey دستی → تقریباً همیشه Profile.

### ۲) همان «برگشت آدرس» را در پورتال SSO ثبت کنید

روی سرور city باز کنید:

`GET https://city.mashhad.ir:5065/api/auth/sso-return-url`

فیلد **`registerInSsoPortal`** را **عیناً** (کپی/پیست) در پورتال SSO برای کاربردی FinancialAssistant بگذارید. معمولاً:

`https://city.mashhad.ir:5065/management`

**بدون** اسلش اضافه در انتها، مگر پورتال فقط با `/management/` قبول کند — آن وقت appsettings هم باید همان باشد (`SsoRegisteredReturnUrl`).

مقدار decode شدهٔ `returnUrl` در Login.aspx شما درست است (`.../management`)؛ اگر باز هم Profile می‌مانید، مشکل از **ثبت پورتال** یا **روش Login.aspx** است، نه از encode URL.

### ۳) loginKey را درست کنید (مهم‌ترین قدم فنی)

`GET https://city.mashhad.ir:5065/api/auth/sso-loginkey-check`

- اگر **`loginKeyOk: false`** یا **403 Client info missmatched**:
  - `ApiName` / `SSOUserName` = **نام کاربری کاربردی** در پورتال (نه عنوان نمایشی برنامه)
  - `ClientId` / `LKey` = همان شناسهٔ `lkey` در URL
  - `ClientSecret` = **SecretKey کامل** (اگر در appsettings کوتاه/نمایشی است، loginKey هرگز OK نمی‌شود)

روی سروری که به `login.mashhad.ir` دسترسی دارد:

`RayvarzResend/scripts/test-mashhad-sso-loginkey.ps1`

وقتی loginKey OK شد، آدرس ورود باید شبیه **`/Authentication/Start/xxxx`** باشد، **نه** `Login.aspx`.

در appsettings پیشنهاد production:

```json
"UseLoginKeyOnRedirect": true,
"AllowLegacyLoginUrlWithoutLoginKey": false
```

تا اگر loginKey خراب است، به‌جای Profile، **خطای واضح** ببینید.

### ۴) آزمایش نام پارامتر returnUrl (فقط اگر پورتال قدیمی است)

گاهی Login.aspx فقط **`ReturnUrl`** (R بزرگ) را می‌پذیرد. یک بار در `Auth:Sso`:

```json
"ReturnUrlParameter": "ReturnUrl"
```

deploy و دوباره از `/auth/login` تست کنید. **هر دو** `returnUrl` و `ReturnUrl` را با هم در URL نفرستید (در کد فقط یکی فرستاده می‌شود).

### ۵) روی Profile.aspx

گاهی لینک «بازگشت به برنامه» یا نام کاربردی روی همان صفحه هست — اگر `returnUrl` از session SSO حذف شده باشد، باز هم به city با توکن نمی‌روید. راه حل پایدار همان **Start/loginKey + ثبت برگشت آدرس** است.

## بعد از درست شدن

آدرس مرورگر باید بشود:

`https://city.mashhad.ir:5065/management?username=...&refresh_token=...`

آن وقت middleware SSO `QueryString` پر می‌بیند و به `TryCompleteSsoCallbackAsync` می‌رود.

مستندات دیباگ: [VS175-SSO-DEBUG-BREAKPOINTS-fa.md](./VS175-SSO-DEBUG-BREAKPOINTS-fa.md)

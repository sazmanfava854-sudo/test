# دیباگ SSO — چرا بعد از لاگین مشهد breakpoint نمی‌خورد؟

این سند برای شاخهٔ **net7 / VS 17.5** (`cursor/net7-vs175-debug-ffcb`) است. جریان SSO در **دو فاز** جدا اجرا می‌شود؛ بیشتر breakpointهایی که گذاشته‌اید در **فاز ۱** هستند، در حالی که بعد از وارد کردن موبایل/کد در `login.mashhad.ir` فقط **فاز ۲** روی سرور **برگشت** اجرا می‌شود.

## فاز ۱ — شروع SSO (قبل از رفتن به login.mashhad.ir)

| فایل | متد | چه زمانی |
|------|-----|----------|
| `ShimasAuthService.cs` | `BuildExternalLoginUrlAsync` | یک بار وقتی `/auth/login` (یا معادل) را باز می‌کنید |
| `MashhadSsoApiClient.cs` | `GetLoginKeyAsync` → `SendSignedJsonAsync` | اگر `UseLoginKeyOnRedirect=true` و API در دسترس باشد |
| `MashhadSsoSigning.cs` | `CreateMaterial` | داخل هر درخواست امضاشدهٔ فاز ۱ (مثلاً loginKey) |

**گیر روی `Profile.aspx` بعد از لاگین:** [SSO-PROFILE-STUCK-fa.md](./SSO-PROFILE-STUCK-fa.md) — `returnUrl` در Login.aspx درست است ولی مشهد به city برنمی‌گرداند؛ اول loginKey و ثبت «برگشت آدرس» را درست کنید.

**نکته:** اگر در آدرس مرورگر **`Login.aspx?lkey=...&returnUrl=...`** می‌بینید (نه `Authentication/Start/{loginKey}`)، یعنی loginKey ناموفق بوده و با `AllowLegacyLoginUrlWithoutLoginKey` به روش قدیمی fallback شده است. در این حالت `CreateMaterial` برای loginKey ممکن است **قبل از redirect** یک بار خورده باشد؛ خط ۱۹۴ (`return BuildLoginStartUrl`) در این سناریو **اجرا نمی‌شود** — به‌جای آن `BuildExternalLoginUrl` (حدود خط ۱۸۳) اجرا می‌شود.

این فاز فقط روی **همان processی** breakpoint می‌خورد که درخواست `/auth/login` را جواب می‌دهد:

- **F5 در Visual Studio** → فقط اگر خودتان `https://localhost:5088/auth/login` (یا همان پورت پروفایل) را باز کنید **و** دیباگر به همان process وصل باشد.
- اگر از **`https://city.mashhad.ir:5065`** وارد می‌شوید → کد روی **IIS/سرویس شهر** است، نه روی Visual Studio لوکال؛ breakpoint در VS **تا وقتی Attach به آن سرور نکنید** نمی‌خورد.

## فاز ۲ — بعد از لاگین در مشهد (callback)

بعد از تأیید OTP/رمز، مرورگر مستقیماً به آدرس **ثبت‌شده در پورتال SSO** می‌رود (معمولاً):

`https://city.mashhad.ir:5065/management?username=...&refresh_token=...`

از این لحظه تا وقتی دوباره به برنامهٔ شما نرسید، **هیچ کدی از RayvarzResend روی ماشین دیباگ شما اجرا نمی‌شود** — فقط سرورهای مشهد.

وقتی درخواست GET به **city** رسید:

| ترتیب | فایل | محل |
|-------|------|-----|
| ۱ | `Program.cs` | middleware ~۱۹۸: `IsSsoCallbackHttpRequest` |
| ۲ | `Program.cs` | `TryCompleteSsoCallbackAsync` (~۱۵۵): `ParseCallbackQuery`, `ValidateReturnedState` |
| ۳ | `ShimasAuthService.cs` | `ValidateAsync` → `ValidateViaMashhadApiAsync` (~۶۲۵) |
| ۴ | `MashhadSsoApiClient.cs` | `GetAccessTokenAsync` → **`SendSignedJsonAsync`** |
| ۵ | `MashhadSsoSigning.cs` | **`CreateMaterial`** (دوباره، برای getAccessToken / getUserInfo) |

پس breakpointهای `CreateMaterial` و `SendSignedJsonAsync` **بعد از لاگین** باید روی **process سرویس‌دهندهٔ city:5065** بخورند، نه روی localhost مگر عمداً callback را به localhost برگردانده باشید (در پورتال SSO معمولاً فقط city ثبت است).

## `await next()` در middleware — طبیعی است یا نه؟

در `Program.cs` ترتیب درست این است (اول `if`، بعد `await next()`):

```csharp
if (shimas.IsSsoCallbackHttpRequest(context.Request)) { ... return; }
await next();
```

اگر دیباگر به **`await next()`** می‌رسد، یعنی **`IsSsoCallbackHttpRequest` = false** برای **این درخواست**. خیلی وقت‌ها درخواست فعلی اصلاً callback نیست (مثلاً `/auth/login`، فایل JS، یا `/management/` بدون query).

برای **همان URL بعد از لاگین مشهد** در Immediate Window یا Watch بزنید:

- `context.Request.Path`
- `context.Request.QueryString`
- `shimas.ProbeSsoCallbackHttpRequest(context.Request).RejectionReasonFa`

یا در مرورگر (با همان querystring):

`GET /api/auth/sso-callback-probe`

دلایل رایج `false`:

| شرط | علامت |
|-----|--------|
| متد | باید **GET** باشد |
| مسیر | باید `/management` (یا `CallbackPath`) باشد — نه فقط `/login.html` |
| توکن | `refresh_token` / `refreshToken` در query با طول ≥ `MinRefreshTokenLength` |
| هویت | حداقل **username** یا **domain** در query |

اگر توکن هست ولی username نیست، middleware دوم ممکن است قبلاً شما را بدون پیام به `/auth/login` می‌فرستاد؛ در نسخهٔ جدید به `login.html?error=...` با دلیل فارسی می‌رود.

## چک‌لیست سریع

1. **دیباگر به کدام process وصل است؟**  
   - شروع SSO از city → فاز ۱ و ۲ هر دو روی **سرور**.  
   - F5 لوکال + دکمهٔ SSO که به city می‌رود → فاز ۱ روی city است؛ فاز ۲ هم روی city.

2. **بعد از لاگین URL مرورگر چیست؟**  
   - اگر هنوز `login.mashhad.ir` یا `Profile.aspx` است → **برگشت SSO نشده** (اغلب `returnUrl` با ثبت پورتال یکی نیست، یا SSO روی Profile مانده).  
   - باید به `city.mashhad.ir:5065/management` (یا `/auth/callback`) با `refresh_token` طولانی برسید.

3. **فاز ۲ اصلاً شروع نشده**  
   Breakpoint در `Program.cs` خط ~۲۰۱ (`if (shimas.IsSsoCallbackHttpRequest`) بگذارید. اگر نمی‌خورد: مسیر یا query با `CallbackPath` و `MinRefreshTokenLength` جور نیست.

4. **فاز ۲ شروع شده ولی قبل از API می‌ایستد**  
   - `ValidateReturnedState` در `TryCompleteSsoCallbackAsync` — اگر state کوکی با query یکی نباشد، redirect به `login.html?error=...` بدون `ValidateViaMashhadApiAsync`.  
   - برای **Login.aspx** بدون state، معمولاً state خالی قبول می‌شود (کد فعلی).

5. **لاگ**  
   `Auth:Shimas:DebugSigning: true` — خروجی امضا و خطاهای loginKey/getAccessToken در لاگ سرور (یا Output در F5).

## پیشنهاد breakpoint برای مشکل «بعد از زدن اطلاعات لاگین»

به‌جای تکیه فقط به `BuildExternalLoginUrlAsync`:

```
Program.cs          → TryCompleteSsoCallbackAsync (خط ~۱۶۸ ValidateAsync)
ShimasAuthService   → ValidateViaMashhadApiAsync (خط ~۶۳۸ GetAccessTokenAsync)
MashhadSsoApiClient → SendSignedJsonAsync
```

و برای اطمینان از رسیدن callback:

```
Program.cs          → if (shimas.IsSsoCallbackHttpRequest(...))  (~۲۰۱)
```

## دیباگ فاز ۲ روی سرور city (پیشنهاد عملی)

1. آخرین بیلد شاخهٔ net7 را روی سرور تست (یا zip دیباگ) deploy کنید.  
2. در Visual Studio: **Debug → Attach to Process** → `w3wp.exe` مربوط به سایت `5065` (یا process Kestrel اگر self-host).  
3. همان breakpointهای فاز ۲ را فعال کنید.  
4. دوباره SSO از `https://city.mashhad.ir:5065/login.html` یا `/auth/login?debug=1`.

صفحهٔ `?debug=1` روی `/auth/login` قبل از redirect، `returnUrl` و اینکه flow **Start/loginKey** یا **Login.aspx** است را نشان می‌دهد.

## APIهای کمکی بدون breakpoint

بعد از Run (لوکال یا سرور):

- `GET /api/auth/sso-loginkey-check` — آیا loginKey از API می‌آید؟  
- `GET /api/auth/sso-loginkey-probe` — جزئیات خطای ۴۰۳/امضا  
- `GET /api/auth/mode` — `registeredCallbackUrl`, `publicSsoLoginUrl`

## خلاصهٔ یک جمله‌ای

**`BuildExternalLoginUrlAsync` و loginKey = قبل از رفتن به مشهد؛ `CreateMaterial`/`SendSignedJsonAsync` بعد از لاگین = روی همان سروری که `returnUrl` (معمولاً city) را جواب می‌دهد — نه لزوماً همان Visual Studio F5 روی localhost.**

مستندات عمومی net7: [VS175-NET7-DEBUG-fa.md](./VS175-NET7-DEBUG-fa.md)

## خطاهای دیباگر: IsCollectible / CustomAttributes / Session

در **Locals** یا **Immediate Window** گاهی این پیام‌ها دیده می‌شود:

- `Method ... get_IsCollectible cannot be called in this context`
- `MemberInfo.get_CustomAttributes cannot be called in this context`
- `'Session' threw an exception of type 'System.InvalidOperationException'`

این‌ها **خطای اجرای برنامه نیستند** — Visual Studio هنگام باز کردن (expand) بعضی propertyها در حالت توقف (breakpoint)، reflection یا `HttpContext.Session` را صدا می‌زند و در آن context مجاز نیست.

| علامت در دیباگر | معنی |
|-----------------|------|
| `IsCollectible` / `CustomAttributes` / `IsConstructedGenericMethod` | روی **MethodInfo** یا delegate باز کرده‌اید — نادیده بگیرید. |
| `Session` روی `HttpContext` | این پروژه **ASP.NET Session** (`UseSession`) ندارد؛ cookie احراز هویت است. expand کردن `context.Session` همیشه خطا می‌دهد — طبیعی است. |

**به‌جای expand کردن `context` یا `Request`، در Watch این‌ها را بزنید (رشته):**

```text
context.Request.Method
context.Request.Path.Value
context.Request.QueryString.Value
context.Request.Host.Value
```

برای SSO (بعد از pull شاخهٔ net7):

```text
shimas.ProbeSsoCallbackHttpRequest(context.Request).RejectionReasonFa
shimas.ProbeSsoCallbackHttpRequest(context.Request).IsSsoCallbackHttpRequest
```

**تنظیم اختیاری VS:** Tools → Options → Debugging → General → خاموش کردن **Enable property evaluation and other implicit function calls** (کمتر خطای عجیب در Locals؛ بعضی propertyها خودکار پر نمی‌شوند).

**جایگزین:** یک خط موقت در `Program.cs` داخل middleware (فقط لوکال) قبل از `await next()`:

```csharp
var _dbg = shimas.ProbeSsoCallbackHttpRequest(context.Request).RejectionReasonFa;
```

روی `_dbg` breakpoint بگذارید — بدون Immediate Window.

# دیباگ SSO: سیستم خودتان (F5) در برابر سرور 140 (IIS)

## دو محیط — یک تصویر ساده

```
[مرورگر شما]
     |
     +-- http://5.252.216.140:8070/...  ----->  IIS روی سرور 140  (Publish)
     |
     +-- http://localhost:5088/...      ----->  Visual Studio F5 روی PC شما  (غیر Publish)
```

**قانون طلایی:** breakpoint فقط جایی می‌خورد که **همان process** درخواست را اجرا کند.

| آدرسی که در مرورگر باز می‌کنید | کد کجا اجرا می‌شود؟ | F5 روی لپ‌تاپ breakpoint می‌خورد؟ |
|--------------------------------|---------------------|-----------------------------------|
| `5.252.216.140:8070` | IIS سرور 140 | **خیر** |
| `localhost:5088` (پورت پروفایل VS) | process دیباگ شما | **بله** |
| `city.mashhad.ir:5065` | IIS شهر (محیط دیگر) | **خیر** (مگر Attach به همان IIS) |

شما از PC به `140:8070` می‌روید = **تست روی سرور Publish** است؛ دیباگر روی لپ‌تاپ به آن وصل نیست.

---

## خطا 403 (Client info missmatched) — نه 402

در SSO مشهد معمولاً **403** می‌بینید (نه 402). یعنی هدرهای `apiName` / `requestTime` / `apiSecret` یا بدنهٔ loginKey با **ثبت پورتال** جور نیست.

اگر **همهٔ فرمول‌ها** در `/api/auth/sso-loginkey-probe` 403 هستند، با step-debug هم فقط **همان مقادیر اشتباه** را می‌بینید — اول **ClientId / SecretKey / apiName** را از ادمین SSO با `appsettings` یکی کنید.

---

## برای دیباگ مرحله‌به‌مرحله در Visual Studio (روی PC خودتان)

### مرحله ۱ — همان اعتبار سرور 140

از سرور 140 (RDP) فایل واقعی را کپی کنید:

`RayvarzResend.Web\appsettings.json` (یا `appsettings.Production.json`)

روی PC در **`appsettings.Development.json`** (یا User Secrets) بلوک `Auth:Shimas` را بگذارید — **Secret را commit نکنید.**

برای **8070** معمولاً:

```json
"PublicBaseUrl": "http://5.252.216.140:8070",
"SsoRegisteredReturnUrl": "http://5.252.216.140:8070/management",
"PreferLocalLoginHosts": [ "5.252.216.140" ],
"AllowSsoOnLoopbackForDebug": true
```

«برگشت آدرس» در **پورتال SSO** باید **عیناً** همان `SsoRegisteredReturnUrl` باشد (یا city اگر فقط city ثبت شده — آن وقت روی 140 SSO درست کار نمی‌کند تا URL ثبت را عوض کنید).

### مرحله ۲ — F5

1. شاخه `cursor/net7-vs175-debug-ffcb`
2. Startup: `RayvarzResend.Web`
3. F5 → مثلاً `http://localhost:5088`

### مرحله ۳ — F10 روی PC (حتی وقتی کاربران `city.mashhad.ir:5065` می‌بینند)

در `appsettings.Development.json` همان `PublicBaseUrl` را بگذارید:

`https://city.mashhad.ir:5065`

(برگشت SSO همان است؛ فقط **درخواست از localhost** اجرا می‌شود تا breakpoint بخورد.)

با F5 معمولاً `http://localhost:5088/` باز می‌شود (بدون `auth/login?debug=1`).

| URL در مرورگر (دستی) | برای F10 |
|----------------------|----------|
| `http://localhost:5088/api/auth/sso-loginkey-check` | ساده‌ترین — مستقیم loginKey |
| `http://localhost:5088/api/auth/sso-signing-preview` | فقط ساخت hash/هدر (بدون POST loginKey) |
| `http://localhost:5088/auth/login?debug=1` | (اختیاری) صفحهٔ تشخیص + ادامه به `/auth/login` |

**نه** `https://city.mashhad.ir:5065/...` در مرورگر اگر F5 روی لپ‌تاپ زده‌اید — آن روی IIS پشت city است.

### مرحله ۴ — breakpoint (هدر loginKey)

| فایل | محل |
|------|-----|
| `MashhadSsoApiClient.cs` | `SendSignedJsonAsync` — بعد از `CreateMaterial` (~208) |
| `MashhadSsoApiClient.cs` | `BuildSignedPost` (~258) — قبل از `SendAsync` |
| `MashhadSsoApiClient.cs` | بعد از `ReadAsStringAsync` (~229) — `body`, `last.ErrorCode` |
| `MashhadSsoSigning.cs` | `CreateMaterial` |

در Watch:

```text
material.ApiName
material.RequestTime
material.ApiSecret
_options.ClientSecret.Length
```

مقایسه با `/api/auth/sso-signing-preview` روی **همان localhost**.

---

## اگر حتماً روی 140 تست می‌کنید

- مرورگر: `http://5.252.216.140:8070/...` → کد روی **IIS 140**
- دیباگ: روی **140** Visual Studio → **Debug → Attach to Process → `w3wp.exe`** (سایت پورت 8070)
- یا Remote Debugging مایکروسافت از PC به 140

بدون Attach، breakpoint روی لپ‌تاپ **هیچ‌وقت** برای 8070 نمی‌خورد.

**نکته:** با `PreferLocalLoginHosts: 5.252.216.140` روی **140** ورود سازمانی ممکن است پیش‌فرض نباشد؛ برای SSO مستقیم بروید:

`http://5.252.216.140:8070/auth/login?debug=1`

---

## city (5065) در برابر 140 (8070)

| محیط | آدرس نمونه | ثبت SSO |
|------|------------|---------|
| شهر | `https://city.mashhad.ir:5065` | برگشت آدرس city |
| داخلی | `http://5.252.216.140:8070` | برگشت آدرس 140 (اگر در پورتال ثبت شده) |

تست city روی 140 یا برعکس بدون ثبت جدا در پورتال → loginKey / Profile خطا می‌دهد.

---

## جمع‌بندی یک جمله

**دیباگ هدر با F5 = PC + `localhost` + همان `appsettings` اعتبار 140.**  
**تست Publish = `140:8070` در مرورگر = Attach به IIS روی 140 یا قبول کنید بدون step-debug فقط API probe بزنید.**

راهنمای 403 بدون دیباگر: [SSO-403-NO-DEBUG-NEEDED-fa.md](./SSO-403-NO-DEBUG-NEEDED-fa.md)

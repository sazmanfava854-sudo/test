# بریک‌پوینت برای خطای loginKey 403 (Client info missmatched)

وقتی `/auth/login?debug=1` می‌گوید **loginKey: ناموفق (403)**، مشکل **قبل از** redirect به Login.aspx و **قبل از** Profile.aspx است: SSO هدر/بدنهٔ `POST /api/Authentication/loginKey` را قبول نمی‌کند.

## کدام process را دیباگ کنید؟

همان جایی که این URL را باز می‌کنید:

`https://city.mashhad.ir:5065/auth/login?debug=1`

یا `GET /api/auth/sso-loginkey-check`

→ معمولاً **IIS روی city** (`w3wp.exe`)، نه F5 روی localhost (چون با `PublicBaseUrl` ممکن است `/auth/login` لوکال به city redirect شود).

**Visual Studio:** Debug → Attach to Process → `w3wp.exe` (سایت 5065) یا همان پروژه را روی سرور با F5.

## چطور loginKey را صدا بزنید (بدون لاگین مشهد)

| عمل | URL |
|-----|-----|
| متن debug + تست loginKey | `GET /auth/login?debug=1` |
| فقط JSON | `GET /api/auth/sso-loginkey-check` |
| hash هدر/بدنه | `GET /api/auth/sso-signing-preview` |
| چند فرمول هش | `GET /api/auth/sso-loginkey-probe` |

بریک‌پوینت را بگذارید و یکی از این آدرس‌ها را در مرورگر refresh کنید.

## ترتیب بریک‌پوینت (از بالا به پایین)

### ۱) ورود به جریان

| فایل | خط (تقریبی) | متد |
|------|-------------|-----|
| `Program.cs` | `var loginUrl = await sso.BuildExternalLoginUrlAsync(...)` | `/auth/login` |
| `Program.cs` | `var diag = await sso.DiagnoseLoginKeyAsync(...)` | همان با `?debug=1` |
| `SsoAuthService.cs` | `var key = await _mashhadSso.GetLoginKeyAsync(...)` | `DiagnoseLoginKeyAsync` |

### ۲) امضا و POST (مهم‌ترین برای 403)

| فایل | خط | چه چیزی ببینید |
|------|-----|----------------|
| `MashhadSsoApiClient.cs` | `SendSignedJsonAsync` — بعد از `CreateMaterial` | `material.ApiName`, `material.RequestTime`, `material.ApiSecret` |
| `MashhadSsoSigning.cs` | `CreateMaterial` — `apiSecret = SsoApiSecretHash.Compute...` | ورودی: `secretKey`, `requestTime`, `hashEncoding`, `concatOrder` |
| `MashhadSsoApiClient.cs` | `var payload = buildPayload(material)` | برای loginKey: `ClientId`, `State`, `UserType`, `DomainId` در بدنه |
| `MashhadSsoApiClient.cs` | `BuildSignedPost` | هدرهای `apiName`, `requestTime`, `apiSecret` |
| `MashhadSsoApiClient.cs` | **بعد از** `ReadAsStringAsync` — `body` و `last.ErrorCode` | اگر `403` و `Client info missmatched` → مقادیر هدر/بدنه با پورتال یکی نیست |

شرط بریک‌پوینت (اختیاری) روی `SendAsync`:

```text
relativePath.Contains("loginKey")
```

### ۳) retry هش (اگر دو بار می‌زند)

| فایل | خط |
|------|-----|
| `MashhadSsoApiClient.cs` | `GetLoginKeyAsync` → `ShouldRetryLoginKeyWithAlternateHash` |
| `MashhadSsoApiClient.cs` | دومین `SendLoginKeyAsync` با `upper`/`lower` |

## Watch (بدون expand روی Session)

```text
_options.SigningApiName
_options.EffectiveClientId
(_options.ClientSecret ?? "").Length
_options.EffectiveLoginKeyBodyClientId
_options.HashEncoding
_options.EffectiveApiSecretConcatOrder
material.ApiSecret
body
last.ErrorCode
last.ErrorMessage
```

**هرگز** `ClientSecret` را در اسکرین‌شات نفرستید؛ فقط **طول** و اینکه با پورتال یکی است.

## 403 یعنی چه؟ (چه چیزی با دیباگ مقایسه کنید)

طبق سند SSO (همان کد `MashhadSsoSigning`):

- هدر **`apiName`** = **نام کاربری کاربردی** در پورتال — **نه** لزوماً عنوان «FinancialAssistant»؛ اگر ادمین SSO نام کاربری دیگری داده، 403 می‌گیرید.
- هدر **`apiSecret`** = `SHA256(SecretKey + requestTime)` با `ApiSecretConcatOrder` (پیش‌فرض `SecretTime`) و `HashEncoding` (`lower`/`upper`).
- بدنه **`ClientId`** = همان شناسهٔ ثبت‌شده (`EffectiveLoginKeyBodyClientId`؛ معمولاً همان `lkey`، نه apiName مگر `LoginKeyBodyClientIdIsApiName=true`).

**بدون بریک‌پوینت:**  
`/api/auth/sso-loginkey-probe` و در صورت RuleEngine: `?profile=settings`  
`/api/auth/sso-loginkey-probe?apiName=نام_کاربری_از_پورتال`

اگر **هیچ** variant OK نشد → Secret/ClientId/apiName در appsettings با پورتال یکی نیست.  
اگر یک variant OK شد → همان `HashEncoding` / `ApiSecretConcatOrder` / `LoginKeyBodyClientIdIsApiName` را در `Auth:Sso` بگذارید.

## بعد از رفع 403

`/auth/login?debug=1` باید بگوید روش ورود **`Authentication/Start/{loginKey}`** — آن وقت Profile.aspx و callback را جدا تست کنید ([SSO-PROFILE-STUCK-fa.md](./SSO-PROFILE-STUCK-fa.md)).

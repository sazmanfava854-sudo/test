# نتیجه مقایسه SSO: RuleEngine (`SSO.cs`) ↔ RayvarzResend

**منبع RuleEngine:** `SSO_1558.cs` / کلاس `SSO` — متدهای `getCurrentTime()` و `GetloginKey()`.

**تاریخ به‌روزرسانی:** ۱۴۰۴ — پس از اصلاح `ApiSecretConcatOrder` و اسکریپت‌های PowerShell.

---

## ۱) فرمول هش (مهم‌ترین تفاوت قبلی)

| | RuleEngine | RayvarzResend (نسخه فعلی) |
|---|------------|---------------------------|
| ورودی SHA256 | `SSOSecret + r.Data` | `ClientSecret + requestTime` |
| تنظیم | ثابت در کد | `Auth:Shimas:ApiSecretConcatOrder` = **`SecretTime`** |
| الگوریتم | `PublicHelper.getHashSha256` — ASCII + hex `x2` | `SsoApiSecretHash.Sha256AsciiHexLower` — همان الگو |

**اشتباه رایج:** `requestTime + Secret` — در RuleEngine **هرگز** این ترتیب نیست.

**مثال:** Secret=`abc`, time=`1790765181` → رشته قبل از هش: `abc1790765181` (نه `1790765181abc`).

---

## ۲) loginKey — هدر و بدنه

| فیلد | RuleEngine | RayvarzResend (`appsettings` / کد) |
|------|------------|-----------------------------------|
| URL | `{SSOBaseUrl}/api/Authentication/loginKey` | `ApiBaseUrl` + همان مسیر |
| هدر `apiName` | `SSOUserName` | `ApiName` یا `SSOUserName` |
| هدر `requestTime` | `r.Data` از getCurrentTime | همان |
| هدر `apiSecret` | `getHashSha256(SSOSecret + r.Data)` | همان مقدار در `Hash` بدنه |
| بدنه `Time` | `r.Data` | همان |
| بدنه `Hash` | همان apiSecret | همان |
| بدنه `ClientId` | `SSOClientId` | `ClientId` / `LKey` — `LoginKeyBodyClientIdIsApiName: false` |
| `State` | `"test"` | `LoginState`: `"test"` |
| `UserType` | `0` | `LoginUserType`: `0` |
| `DomainID` | `0` | `LoginDomainId`: `0` |
| `ReturnUrl` | **ارسال نمی‌شود** | `IncludeReturnUrlInLoginKey`: **false** |

---

## ۳) HTTP و شبکه

| مورد | RuleEngine | RayvarzResend |
|------|------------|---------------|
| getCurrentTime | RestSharp GET، بدون هدر امضا | `GET /api/Authentication/getCurrentTime` |
| پروکسی | `HttpClientHandler.UseProxy = false` | `AddHttpClient(MashhadSsoApi)` با `UseProxy = false` |
| Content-Type | `application/json` | `application/json; charset=utf-8` |

---

## ۴) بعد از loginKey

| | RuleEngine | RayvarzResend |
|---|------------|---------------|
| redirect | `Authentication/Start/{loginKey}` | `LoginStartUrlTemplate` + `UseLoginKeyOnRedirect: true` |
| getAccessToken | هش همان `SSOSecret + time`؛ بدنه `ClientID` | `MashhadAccessTokenRequest` با `[JsonPropertyName("ClientID")]` |

---

## ۵) تنظیم پیشنهادی FinancialAssistant (RayvarzResend)

```json
"Shimas": {
  "ApiBaseUrl": "https://login.mashhad.ir",
  "ApiName": "FinancialAssistant",
  "SSOUserName": "FinancialAssistant",
  "ClientId": "53db42619cf3C333b13a18D34fbd9111",
  "LKey": "53db42619cf3C333b13a18D34fbd9111",
  "ClientSecret": "<Secret ثبت‌شده در پورتال>",
  "HashEncoding": "lower",
  "ApiSecretConcatOrder": "SecretTime",
  "LoginKeyBodyClientIdIsApiName": false,
  "IncludeReturnUrlInLoginKey": false,
  "UseLoginKeyOnRedirect": true,
  "LoginState": "test",
  "LoginUserType": 0,
  "LoginDomainId": 0
}
```

---

## ۶) اعتبار SSO (چرا RuleEngine OK و RayvarzResend 403؟)

RuleEngine با **`SSOUserName` / `SSOClientId` / `SSOSecret` سامانهٔ خودش** ثبت شده است.

RayvarzResend باید trio **FinancialAssistant** را از پورتال SSO بگیرد — **کپی settings RuleEngine (مثلاً zavabetapp) روی FinancialAssistant کار نمی‌کند.**

---

## ۷) تست روی ویندوز (وقتی PowerShell timeout می‌دهد)

| فایل | کاربرد |
|------|--------|
| `sso-loginkey-curl-only.ps1` | فقط `curl.exe` — هش مثل RuleEngine (`Secret+time`) |
| `run-sso-loginkey-curl-only.cmd` | اجرا بدون تغییر Execution Policy |

دانلود مستقیم از repo (شاخه `cursor/unified-excel-epay-release-ffcb`):

- `RayvarzResend/docs/RULEENGINE-SSO-PARITY.md` (این فایل)
- `RayvarzResend/scripts/sso-loginkey-curl-only.ps1`

---

## ۸) فایل‌های کد مرتبط در RayvarzResend

- `Services/SsoApiSecretHash.cs` — هش
- `Services/MashhadSsoSigning.cs` — امضای درخواست
- `Services/MashhadSsoApiClient.cs` — loginKey / token
- `Services/MashhadSsoJson.cs` — نام فیلدهای JSON (`DomainID`, `ClientID`)
- `Tests/RuleEngineSsoParityTests.cs` — تست هم‌ترازی با `SSOSecret + time`

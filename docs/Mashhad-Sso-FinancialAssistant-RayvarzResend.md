# Mashhad SSO — FinancialAssistant & RayvarzResend

RuleEngineDocuments is the **reference**. FinancialAssistant and RayvarzResend must use the **same** header/body rules.

## Configuration (appsettings)

```json
"MashhadSso": {
  "SSOBaseUrl": "https://login.mashhad.ir",
  "SSOUserName": "FinancialAssistant",
  "SSOClientId": "53db42619cf3C333b13a18D34fbd9111",
  "SSOSecret": "FROM_SSO_PORTAL"
}
```

| Setting | Used as |
|---------|---------|
| `SSOUserName` | HTTP header **`apiName`** only |
| `SSOClientId` | JSON body **`ClientId`** on loginKey; body **`ClientID`** on getAccessToken |
| `SSOSecret` | Never sent plain — only `SHA256(SSOSecret + requestTime)` → header **`apiSecret`** and body **`Hash`** |

## Doc rules (sections 1–3)

1. **`requestTime`** = `getCurrentTime().Data` (fresh on each API call).
2. Only **`getCurrentTime`** has no auth headers.
3. **`apiSecret`** = SHA256(`SSOSecret` + `requestTime`); do not send SecretKey in clear.

## Shared client in this repo

Copy or reference project:

`samples/MashhadSso/` → `MashhadSsoClient.cs`, `MashhadSsoOptions.cs`, `CryptoHelper.cs`

## FinancialAssistant — apply steps

1. Add project reference to `MashhadSso` (or copy sources into your solution).
2. Bind `MashhadSsoOptions` from configuration section `MashhadSso`.
3. Register `MashhadSsoClient` (singleton or typed HttpClient).
4. **Login start:** call `GetLoginKeyAsync(state)` → redirect to `GetAuthenticationStartUrl(loginKey)`.
5. **Callback (Return URL):** read `username`, `refresh_token`, `state` from query → `GetAccessTokenAsync(refresh_token, username)` → store `AccessToken`.
6. Remove any code that sets header `apiName` to `SSOClientId` (GUID) — must be **`SSOUserName`**.

## RayvarzResend — apply steps

1. Same `MashhadSso` configuration block (same app registration if same SSO client).
2. If resend runs **after user SSO login**, reuse stored tokens or call `GetAccessToken` on callback path — do not reimplement headers differently.
3. If a background job calls SSO APIs, use `MashhadSsoClient` for every call (fresh `getCurrentTime` per request).

## PowerShell test (same pattern)

`scripts/Test-SsoLoginKey.ps1` — parameters `SSOUserName`, `SSOClientId`, `SSOSecret`.

## 403 Client info missmatched

Layout matches RuleEngine; error means portal **SSOUserName + SSOClientId + SSOSecret** do not match registration. Fix with SSO admin, not by removing `ClientId` from body.

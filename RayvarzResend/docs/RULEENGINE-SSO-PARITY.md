# هم‌ترازی SSO: RuleEngine ↔ RayvarzResend

منبع RuleEngine: `SSO.cs` — `GetloginKey()` / `getCurrentTime()`.

| مورد | RuleEngine | RayvarzResend (`Auth:Shimas`) |
|------|------------|-------------------------------|
| Base URL | `SSOBaseUrl` | `ApiBaseUrl` |
| هدر `apiName` | `SSOUserName` | `ApiName` / `SSOUserName` |
| بدنه `ClientId` | `SSOClientId` | `ClientId` / `LKey` (`LoginKeyBodyClientIdIsApiName: false`) |
| Secret | `SSOSecret` | `ClientSecret` |
| هش | `getHashSha256(SSOSecret + r.Data)` | `SsoApiSecretHash` + `ApiSecretConcatOrder: SecretTime` |
| `State` | `"test"` | `LoginState` (پیش‌فرض test) |
| `UserType` / `DomainID` | `0` / `0` | `LoginUserType` / `LoginDomainId` |
| پروکسی | `UseProxy = false` | `HttpClient` MashhadSso با `UseProxy = false` |
| `ReturnUrl` در loginKey | نمی‌فرستد | `IncludeReturnUrlInLoginKey: false` (مثل RuleEngine) |
| redirect | Start/{loginKey} | `UseLoginKeyOnRedirect: true` + `LoginStartUrlTemplate` |

**اشتباه رایج:** هش `requestTime + Secret` — در RuleEngine همیشه **Secret + time** است (خط `SSOSecret + r.Data`).

**اعتبار:** RuleEngine برای سامانهٔ خودش (`zavabetapp` و …) است؛ FinancialAssistant باید **همان trio ثبت‌شده در پورتال** را در appsettings داشته باشد، نه کپی settings سامانهٔ دیگر.

# Visual Studio 17.5 — شاخهٔ net7.0 (فقط دیباگ لاگین)

این شاخه **جدا از main/net8** است. روی سرور یا zip اصلی (`net8.0`) بدون هماهنگی عوض نکنید.

## چرا net7؟

- پروژهٔ اصلی `net8.0` است و با SDK 7.0.x خطای **NETSDK1045** می‌دهد.
- این شاخه `TargetFramework` را **net7.0** می‌گذارد تا با **Visual Studio 17.5** و SDK نصب‌شدهٔ 7.0.203 بتوانید **F5** بزنید و لاگین SSO را دیباگ کنید.

## SDK و global.json

در `RayvarzResend/global.json`:

- حداقل SDK: **7.0.203**
- `rollForward: latestMajor` — اگر **SDK 8** (مثلاً 8.0.423) نصب باشد، برای **کامپایل** از آن استفاده می‌شود (کد از C# 12 استفاده می‌کند).
- اگر **فقط** SDK 7 دارید و بیلد با خطای `Invalid option '12' for /langversion` می‌خورد، SDK 8 را نصب کنید یا از همان ماشین با `dotnet --list-sdks` مطمئن شوید 8.x موجود است.

## باز کردن در Visual Studio

1. شاخه: `cursor/net7-vs175-debug-ffcb`
2. Solution: `RayvarzResend/RayvarzResend.sln`
3. Startup project: **RayvarzResend.Web**
4. پروفایل: **http** یا **RayvarzResend.Web** — معمولاً `http://localhost:5088`
5. برای لاگ SSO در `appsettings.json` (یا `appsettings.Development.json`):

```json
"Auth": {
  "Shimas": {
    "DebugSigning": true,
    "ApiSecretConcatOrder": "SecretTime",
    "LoginKeyBodyClientIdIsApiName": false
  }
}
```

6. Breakpoint پیشنهادی: `ShimasAuthService`, `MashhadSsoApiClient`, `SsoApiSecretHash`

## APIهای کمکی (بعد از Run)

- `GET /api/auth/sso-outbound-map`
- `GET /api/auth/sso-signing-preview`
- `GET /api/auth/sso-loginkey-check`
- `GET /api/auth/sso-loginkey-probe`

## zip دیباگ (اختیاری)

از پوشهٔ `RayvarzResend/scripts`:

```bash
./build-debug-zip.sh ../RayvarzResend-26-net7-debug.zip
```

خروجی همان اسکریپت net8 است؛ فقط runtime **net7.0** است.

## بازگشت به نسخهٔ اصلی

شاخهٔ release اصلی: `cursor/unified-excel-epay-release-ffcb` — **net8.0**، بدون تغییر این شاخه.

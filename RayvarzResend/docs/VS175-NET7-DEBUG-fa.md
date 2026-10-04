# Visual Studio 17.5 — شاخهٔ net7.0 (فقط دیباگ لاگین)

این شاخه **جدا از main/net8** است. روی سرور یا zip اصلی (`net8.0`) بدون هماهنگی عوض نکنید.

## چرا net7؟

- پروژهٔ اصلی `net8.0` است و با SDK 7.0.x خطای **NETSDK1045** می‌دهد.
- این شاخه `TargetFramework` را **net7.0** می‌گذارد تا با **Visual Studio 17.5** و SDK نصب‌شدهٔ 7.0.203 بتوانید **F5** بزنید و لاگین SSO را دیباگ کنید.

## SDK (بدون global.json)

این شاخه **`global.json` ندارد** — عمداً، تا Visual Studio همان SDK نصب‌شده روی ویندوز را انتخاب کند و خطای **MSB4236** (`Microsoft.NET.Sdk.Web` could not be found) به‌خاطر نسخهٔ اشتباه در `global.json` پیش نیاید.

- **net7.0** با **SDK 7.0.x** (مثلاً 7.0.203) — همان چیزی که VS 17.5 معمولاً استفاده می‌کند — بیلد می‌شود.
- کد این شاخه **C# 11** است (بدون `LangVersion 12`). خطای **CS1617** (`Invalid option '12' for /langversion`) در نسخهٔ به‌روز شاخه برطرف شده است.

### خطای «The SDK Microsoft.NET.Sdk.Web specified could not be found»

1. Solution را باز کنید: `RayvarzResend\RayvarzResend.sln` (نه فقط یک `.csproj` از پوشهٔ دیگر).
2. در **Developer PowerShell** یا CMD:
   ```bat
   cd مسیر\RayvarzResend
   dotnet --list-sdks
   dotnet restore RayvarzResend.sln
   ```
   باید حداقل یک خط `7.0.x` یا `8.0.x` ببینید.
3. اگر `dotnet` شناخته نشد یا لیست خالی است: از Visual Studio Installer → **Modify** → workload **ASP.NET and web development** و **.NET desktop development** را فعال کنید؛ یا SDK 7/8 را جدا نصب کنید.
4. اگر از zip قدیمی استفاده می‌کنید و هنوز `RayvarzResend\global.json` دارید که `7.0.203` می‌گوید: آن فایل را **حذف** کنید یا آخرین zip/شاخه را بگیرید.
5. در `C:\Users\...\` یا `Downloads` اگر `global.json` دیگری هست که SDK را قفل کرده، موقتاً rename کنید.

## باز کردن در Visual Studio

1. شاخه: `cursor/net7-vs175-debug-ffcb`
2. Solution: `RayvarzResend/RayvarzResend.sln`
3. Startup project: **RayvarzResend.Web**
4. پروفایل: **http** یا **RayvarzResend.Web** — معمولاً `http://localhost:5088`
5. **ورود سازمانی** باید از آدرس ثبت SSO باشد، نه localhost:
   - `Auth:Shimas:PublicBaseUrl` = `https://city.mashhad.ir:5065`
   - روی `login.html` لوکال، دکمه «ورود سازمانی» به **`https://city.mashhad.ir:5065/auth/login`** می‌رود (از `/api/auth/mode` → `publicSsoLoginUrl`).
   - برای تست واقعی SSO همان آدرس شهر را در مرورگر باز کنید: `https://city.mashhad.ir:5065/login.html` یا مستقیم `/auth/login?debug=1`
   - F5 روی localhost فقط برای دیباگ کد backend (breakpoint قبل از redirect) است؛ callback SSO به `/management` روی **city** برمی‌گردد.
6. برای لاگ SSO در `appsettings.json`:

```json
"Auth": {
  "Shimas": {
    "DebugSigning": true,
    "ApiSecretConcatOrder": "SecretTime",
    "LoginKeyBodyClientIdIsApiName": false
  }
}
```

7. Breakpoint پیشنهادی: `ShimasAuthService`, `MashhadSsoApiClient`, `SsoApiSecretHash`

### خروج با code 1 و JsonReaderException / InvalidDataException

یعنی **`appsettings.json` JSON معتبر نیست** (قبل از بالا آمدن سایت).

1. فایل: `RayvarzResend.Web\appsettings.json` (همان که در Output کپی می‌شود).
2. در Visual Studio پنجره **Output** → Show output from: **Debug** — متن فارسی خط (شماره خط) را ببینید.
3. علت‌های رایج:
   - بعد از بستن `ConnectionStrings` **ویرگول** جا مانده نیست → باید `},` و بعد `"Auth":`
   - رمز SQL داخل رشته **`"`** یا **`\`** دارد → escape: `\"` و `\\`
   - یک `{` یا `}` کم/زیاد
4. اعتبارسنجی سریع در PowerShell (از پوشه Web):
   ```powershell
   Get-Content appsettings.json -Raw | ConvertFrom-Json
   ```
   اگر خطا داد، همان خط را در JSON اصلاح کنید.
5. برای دیباگ SSO می‌توانید فقط بخش `Auth:Shimas` را عوض کنید؛ `ConnectionStrings` را از zip سالم کپی کنید و فقط Server/Password را عوض کنید.

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

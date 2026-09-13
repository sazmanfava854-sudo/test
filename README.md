# RuleTrace — Sara Formula Debugger (UI)

دیباگر فرمول‌های Sara بدون UI اصلی. کاربر NidProc/NidWorkItem را وارد می‌کند، فرمول (Solh, Rule, Income, ...) اجرا می‌شود و خروجی `AddError` / `BizErrors` و مقدار متغیر Watch نمایش داده می‌شود.

## ساختار

```
ruletrace/
  RuleTrace.sln
  RuleTrace.csproj      ← WinForms, .NET 4.7.2, بدون reference به DLLهای Sara
  Program.cs            ← نقطه شروع
  MainForm.cs           ← UI
  FormulaEngine.cs      ← بارگذاری DLLها در زمان اجرا (reflection) + RunRule
  UserSettings.cs       ← ذخیره تنظیمات UI در bin\RuleTrace.user.ini
  MemberAnalyzer.cs     ← تحلیل جدول Member
  App.config            ← مقادیر پیش‌فرض
  build.cmd             ← Build خودکار + اجرا
  run.cmd               ← اجرای bin\RuleTrace.exe
  bin\                  ← بعد از Build ساخته می‌شود
```

**نکته مهم:** پروژه هیچ reference به `BIZ.SC.DLL` / `SafaClassDesingerNew.dll` ندارد. DLLها **در زمان اجرا** از پوشه‌ای که در UI مشخص می‌کنید (پیش‌فرض `Desktop\dll10`) بارگذاری می‌شوند. بنابراین Build همیشه موفق است.

## Build (یک دستور)

```cmd
cd C:\Users\sadathoseini-sh\Downloads\ruletrace
build.cmd
```

- MSBuild را خودکار پیدا می‌کند (VS 2019/2022/Build Tools/dotnet).
- خروجی: `bin\RuleTrace.exe` و بعد از Build خودکار اجرا می‌شود.

در PowerShell: `.\build.cmd`

## استفاده

1. **پوشه DLL**: `C:\Users\sadathoseini-sh\Desktop\dll10` (خودکار پیدا می‌شود؛ یا «انتخاب پوشه»)
2. **RuleEngine / Sara**: connection stringها با `debugger` (از App.config پر شده)
3. **CityGuid**: `06065CA7-8B68-491F-A002-2AC9CAC8AE34` (خالی = از `CI_City` خوانده می‌شود)
4. **تست اتصال DB** → باید `[RuleEngine] OK` و `[Sara] OK` ببینید
5. **NidWorkItem** (مثلاً `11314989`) یا کد نوسازی → «جستجو در Sara» → NidProc پر می‌شود
6. فرمول `Solh`، Watch `Calc_Chandganeh`
7. **اجرا و دیباگ**

اولین بار **Recompile** را تیک بزنید (چند دقیقه). Cache در `پوشه Cache محلی` ساخته می‌شود. بارهای بعد بدون Recompile.

## Cache محلی

```
{CachePath}\{CityGuid}\{NidRuleClass}\
```

پیش‌فرض: `Desktop\dll10\SafaFormulaCache\06065CA7-...\344\`

## خطاهای رایج

| خطا | علت / راه‌حل |
|-----|--------------|
| `Login failed for user 'hService'` | فایل‌های `*.dll.config` کنار DLLها — RuleTrace خودکار آن‌ها را `.bak` می‌کند |
| `BC2017 could not find c:\dll10\BIZ.SC.DLL` | vbc داخل موتور به `c:\dll10` نیاز دارد — RuleTrace خودکار sync می‌کند؛ اگر دسترسی نبود یک بار as Administrator اجرا کنید |
| `BC30269 'Out' has multiple definitions` / `M_Out` | نسخه `SafaClassDesingerNew.dll` با دیتابیس هم‌خوان نیست — DLLها را دقیقاً از سرور Sara کپی کنید. «تحلیل Member» را بزنید و خروجی را بفرستید |
| `RunRule returned null` | اتصال RuleEngine یا `CnRuleString` |

## اجرای مستقیم با پارامتر (اختیاری)

```cmd
bin\RuleTrace.exe --nidproc FA77A442-29CD-4DDC-ADEA-A3D3A6183F28 --formula Solh --watch Calc_Chandganeh
```

فقط فیلدها را پر می‌کند؛ اجرا با دکمه.

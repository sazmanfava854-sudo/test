# RuleTrace — Sara Formula Debugger (UI)

دیباگر فرمول‌های Sara بدون UI اصلی. کاربر NidProc/NidWorkItem را وارد می‌کند، فرمول (Solh, Rule, Income, ...) اجرا می‌شود و خروجی `AddError` / `BizErrors` و مقدار متغیر Watch نمایش داده می‌شود.

## ساختار

```
ruletrace/
  RuleTrace.sln
  RuleTrace.csproj      ← WinForms, .NET 4.7.2, بدون reference به DLLهای Sara
  Program.cs            ← نقطه شروع
  MainForm.cs           ← UI
  FormulaEngine.cs      ← بارگذاری DLLها در زمان اجرا (reflection) + RunRule + Inspect موتور
  DebugPanel.cs         ← تب «دیباگ مرحله‌ای» (F10 / Shift+F10 / F5) روی trace فرمول
  UserSettings.cs       ← ذخیره تنظیمات UI در bin\RuleTrace.user.ini
  MemberAnalyzer.cs     ← تحلیل جدول Member (نسخه‌ها، ساختار XML)
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

## دیباگ مرحله‌ای (F10) — تب «دیباگ مرحله‌ای»

فرمول‌های Sara کد VB هستند که موتور `SafaClassDesingerNew` در زمان اجرا کامپایل می‌کند؛ نمی‌توان مانند VS روی هر خط breakpoint گذاشت. به جای آن RuleTrace **اجرا را ضبط می‌کند و بعد مرحله‌به‌مرحله بازپخش می‌کند**:

- هر `AddError` که فرمول ثبت می‌کند (همان `BizErrors`) یک **رویداد / گام** است.
- بعد از اجرا، RuleTrace کد VB همه ردیف‌های `dbo.Member` را از DB می‌خواند و برای هر گام **خط متناظر در کد** را highlight می‌کند (خط `AddError("Key", ...)` با همان Key؛ در حلقه‌ها گام n به n‌امین رخداد می‌رود).
- برای هر گام مقدار متغیر (`ParametersValue[Key]`) و مقدار Watch نمایش داده می‌شود.

| کلید | عمل |
|------|-----|
| `F5` | اجرای فرمول (اگر trace وجود دارد و تب دیباگ فعال است: رفتن به آخر) |
| `F10` / `F11` | گام بعدی |
| `Shift+F10` | گام قبلی |
| `Ctrl+Home` | گام اول |

اگر Key در کد پیدا نشد (مثلاً Key به صورت متغیر ساخته می‌شود) فقط رویداد و مقدارها نمایش داده می‌شود. برای دیدن کد بدون اجرا: «بارگذاری کد از DB».

**Breakpoint واقعی (اختیاری):** Visual Studio → Debug → Attach to Process → `RuleTrace.exe`، سپس اجرا. VS روی exception‌های داخل فرمول متوقف می‌شود؛ stepping خط‌به‌خط فقط اگر موتور با `/debug` کامپایل کند ممکن است.

## تشخیص خطای کامپایل (BC30269 / M_Out)

خروجی فعلی: فایل merge‌شده فقط ~۳۳۰ خط است در حالی که ۲۰ XML حدود ۳.۶ MB است و خطاها هر ۱۶ خط تکرار می‌شوند؛ یعنی موتور برای هر Member فقط **پوسته کلاس** را می‌نویسد و بدنه کد را نمی‌خواند. ابزارها:

- **تحلیل Member**: نسخه‌ها/`isActive` هر `NidMember`، حجم `Body` / `XmlBody` / `EncryptXmlBody` و ساختار عناصر XML (کجا کد است، کجا نام).
- **بررسی موتور (ClsClass)**: `ClsClass(344, CityGuid, false)` را دقیقاً مانند `RunRule` می‌سازد و لیست Memberهایی که موتور خوانده (نام، اندازه Body، نسخه) و فیلدهای static `ClsCommon` را چاپ می‌کند. بعد از هر خطای کامپایل خودکار اجرا می‌شود.

**علت رایج (تأیید شده با تحلیل Member شما):** کد VB در `XmlBody/<Body>` خوانا است (~۳۶۰K کاراکتر) اما `EncryptXmlBody` هم پر است؛ موتور `2012.5` هنگام compile از مسیر رمزنگاری می‌خواند، `ClsFunction.Body` خالی می‌ماند و ۲۰ پوسته `M_Out` کنار هم merge می‌شود.

RuleTrace بعد از `BC30269` خودکار **retry** می‌کند: متن `<Body>` را از DB inject می‌کند، پوسته `ToString1` را یک‌بار می‌گیرد، فقط `Sub`/`Function`ها را merge می‌کند (`RuleTrace_merged.vb` در cache) و با **vbc** کامپایل می‌کند (نه `RunRule` دوباره). در Log باید ببینید:

```
RuleTrace merge-v15-class-scope-fix ... — relocate pre-Class member code into class body
Merge source : ToString1 len=~1021416
Merge struct : moved N line(s) from before Class into the class body
Merge params : ... parameter properties added
Merged VB    : ... M_Out decls=1, Property Out=1 (injected path)
Retry compile: vbc (VBCodeProvider) on merged source...
VBC OK       : N_Solh.Solh -> ...\Solh_ruletrace.dll
```

اگر هنوز `GetStrOutClass len=1226` یا `Compile try : RunRule` می‌بینید، ZIP/branch قدیمی است — از `cursor/ruletrace-standalone-88fc` دوباره `build.cmd` بزنید.

اگر retry هم خطا داد: DLL دقیق سرور Sara یا کلید `FormulaEncryptionCode` سرور لازم است.

## اخطار آنتی‌ویروس

`RuleTrace.exe` امضای دیجیتال ندارد و در زمان اجرا DLL بارگذاری می‌کند، به `c:\dll10` کپی می‌کند و موتور Sara فایل `.vb` موقت کامپایل می‌کند — همین رفتار برای heuristics آنتی‌ویروس «مشکوک» است (false positive). راه‌حل: پوشه پروژه را Exclude کنید (Windows Security → Virus & threat protection → Exclusions) یا در PowerShell (Admin):

```powershell
Add-MpPreference -ExclusionPath "C:\Users\sadathoseini-sh\Downloads\rule5"
Add-MpPreference -ExclusionPath "C:\Users\sadathoseini-sh\Desktop\dll10"
```

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
| `BC30269 'Out' has multiple definitions` / `M_Out` | موتور بدنه Memberها را نمی‌خواند (بخش «تشخیص خطای کامپایل») — خروجی «تحلیل Member» و «بررسی موتور» را بفرستید؛ احتمالاً DLL دقیق سرور Sara لازم است |
| اخطار آنتی‌ویروس | false positive (exe بدون امضا + کامپایل داینامیک) — پوشه را Exclude کنید (بخش «اخطار آنتی‌ویروس») |
| `RunRule returned null` | اتصال RuleEngine یا `CnRuleString` |

## اجرای مستقیم با پارامتر (اختیاری)

```cmd
bin\RuleTrace.exe --nidproc FA77A442-29CD-4DDC-ADEA-A3D3A6183F28 --formula Solh --watch Calc_Chandganeh
```

فقط فیلدها را پر می‌کند؛ اجرا با دکمه.

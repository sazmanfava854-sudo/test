# RuleTrace — Sara Formula Debugger (UI)

**نصب:** فایل `RuleTrace.zip` را از ریشه همین پروژه دانلود کنید (نه دکمه سبز GitHub «Code → Download ZIP»). Extract به `C:\ruletrace`. باید `C:\ruletrace\RuleTrace.sln` را مستقیم ببینید. بعد Visual Studio → Rebuild (`Ctrl+Shift+B`).

دیباگر فرمول‌های Sara بدون UI اصلی.

دیباگر فرمول‌های Sara بدون UI اصلی.

## مرحله ۱ — ترکیب کد RuleEngine + DLL (فقط خواندن)

این برنامه **عیب‌یابی** است. کد `dbo.Member` را از RuleEngine کنار نوع‌های DLL می‌گذارد. **چیزی در دیتابیس ذخیره نمی‌شود.**

## مرحله ۲ — اجرا بدون بازنویسی VB (`v21c-cross-class`)

RuleTrace دیگر کد Memberها را به یک فایل VB نمی‌چسباند.

کلاس‌های فرمول **به هم وصل‌اند**. Member چیدمان `1288` در `ZabetehConvert` (NidClass **342**) است، نه در Solh/344. با انتخاب Solh این کلاس‌ها با هم خوانده می‌شوند: Rule 336، ZabetehConvert 342، Solh 344، Tavafogh 345، Global 432.

`build.cmd` — عنوان باید `v21c-cross-class` باشد.

RuleTrace دیگر کد Memberها را به یک فایل VB نمی‌چسباند و `Compile(ToString1)` / vbc نمی‌کند. آن معماری خطاهای BC30269 (Out/M_Out تکراری) و BC30289 (متد داخل متد) می‌ساخت و همگرا نمی‌شد.

اجرا یعنی:

1. `ClsCommon.RunRule` همان موتور Sara
2. اگر `Instanc` ساخته شد → `SetMyInfo` + `Run`
3. اگر نه → جستجوی DLL از قبل کامپایل‌شده در Cache / `dll10` (`*Solh*.dll` / پوشه `344`)
4. اگر هیچ‌کدام نبود → تحلیل ایستای Member 1288 (چیدمان / If Solh / Exit) بدون اجرا — در **کپی خلاصه** با پیشوند `Chidman`

برای اجرای زنده: یک‌بار Solh را در **UI سارا** کامپایل کنید تا DLL در Cache ساخته شود، بعد RuleTrace همان را لود می‌کند.

`build.cmd` — عنوان پنجره باید `v21b-chidman-summary` باشد. NidProc را پر کنید. **ReCompile خاموش.**

اگر Instanc نبود (کد خروج ۲): تب دیباگ روی Member 1288 باز می‌شود. خروجی **کپی خلاصه خطا** باید خطوط `Arch` و `Chidman` را نشان دهد — نه ۵۸ خط `Engine err`.

---

## مرحله ۲+ — اجرا و دیباگ

کاربر NidProc/NidWorkItem را وارد می‌کند، فرمول (Solh, Rule, Income, ...) اجرا می‌شود و خروجی `AddError` / `BizErrors` و مقدار متغیر Watch نمایش داده می‌شود.

## ساختار

```
ruletrace/
  RuleTrace.sln
  RuleTrace.csproj      ← WinForms, .NET 4.7.2, بدون reference به DLLهای Sara
  Program.cs            ← نقطه شروع
  MainForm.cs           ← UI
  CodeEditorPanel.cs    ← مرحله ۱: مشاهده کد Member از RuleEngine + نوع‌های DLL (فقط خواندن)
  MemberRepository.cs   ← خواندن dbo.Member (XmlBody/Body)
  FormulaEngine.cs      ← بارگذاری DLLها در زمان اجرا (reflection) + RunRule یا host DLL از Cache
  ChidmanAnalyzer.cs    ← تحلیل ایستای Member 1288 (چیدمان / Solh guards) بدون کامپایل VB
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

مسیر `Downloads\ruletrace` را در cmd ننویسید — اگر آن پوشه نباشد ویندوز می‌گوید cannot find the path.

1. در Explorer پوشه‌ای را باز کنید که **همین فایل‌ها کنار هم** هستند: `build.cmd`، `RuleTrace.sln`، `RuleTrace.csproj`
2. روی `build.cmd` دابل‌کلیک کنید
3. باید چاپ شود `Folder: ...` و بعد `OK -> ...\bin\RuleTrace.exe`
4. عنوان پنجره: `v21c-cross-class`

اگر ویندوز گفت cannot find پوشهٔ `(47)`: پرانتز مسیر را خراب می‌کند و ZIP دو لایه است. `build.cmd` را از Downloads اجرا نکنید.

کل محتویات پوشهٔ **داخلی** (جایی که `RuleTrace.sln` هست) را در Explorer به `C:\ruletrace` کپی کنید، بعد در Visual Studio همان `C:\ruletrace\RuleTrace.sln` را باز کنید → Ctrl+Shift+B → Ctrl+F5.

## استفاده

1. **پوشه DLL**: `C:\Users\sadathoseini-sh\Desktop\dll10` (خودکار پیدا می‌شود؛ یا «انتخاب پوشه»)
2. **RuleEngine / Sara**: connection stringها با `debugger` (از App.config پر شده)
3. **CityGuid**: `06065CA7-8B68-491F-A002-2AC9CAC8AE34` (خالی = از `CI_City` خوانده می‌شود)
4. **تست اتصال DB** → باید `[RuleEngine] OK` و `[Sara] OK` ببینید
5. **NidWorkItem** (مثلاً `11314989`) یا کد نوسازی → «جستجو در Sara» → NidProc پر می‌شود
6. فرمول `Solh`، Watch `Calc_Chandganeh`
7. **اجرا (موتور Sara)** — ReCompile را خاموش بگذارید

اگر Instanc ساخته نشد، تب دیباگ Member 1288 را نشان می‌دهد. برای اجرای زنده، یک‌بار Solh را در UI سارا کامپایل کنید.

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

موتور Sara هنگام `RunRule` اغلب **پوسته خالی** می‌سازد: `EncryptXmlBody` پر است، `ClsFunction.Body` خالی می‌ماند، ۲۰ پوسته `M_Out` کنار هم merge می‌شود → BC30269.

RuleTrace **دیگر این پوسته را sanitize / Compile / vbc نمی‌کند.** آن حلقه به BC30289 («Statement cannot appear within a method body») می‌رسید و تمام نمی‌شد.

عیب‌یابی چیدمان صلح بدون کامپایل:

- دکمه **تحلیل چیدمان (1288)** یا اجرای Solh وقتی Instanc نیست
- If/Exit نزدیک `InsertChidman` و شرط‌های `Solh` / `صلح` / `GetPeace`
- تب دیباگ: کد Member 1288 از `dbo.Member.XmlBody`

برای اجرای زنده: DLL کامپایل‌شده Sara در

```
Desktop\dll10\SafaFormulaCache\{CityGuid}\344\
```

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
| `BC30269 'Out' has multiple definitions` / `M_Out` | موتور پوسته خالی ساخت — RuleTrace دیگر VB را بازنویسی نمی‌کند؛ Solh را یک‌بار در UI سارا کامپایل کنید یا تحلیل ایستای 1288 را ببینید |
| اخطار آنتی‌ویروس | false positive (exe بدون امضا + کامپایل داینامیک) — پوشه را Exclude کنید (بخش «اخطار آنتی‌ویروس») |
| `RunRule returned null` | اتصال RuleEngine یا `CnRuleString` |

## اجرای مستقیم با پارامتر (اختیاری)

```cmd
bin\RuleTrace.exe --nidproc FA77A442-29CD-4DDC-ADEA-A3D3A6183F28 --formula Solh --watch Calc_Chandganeh
```

فقط فیلدها را پر می‌کند؛ اجرا با دکمه.

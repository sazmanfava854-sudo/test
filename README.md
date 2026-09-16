# RuleTrace — وب‌اپ عیب‌یاب فرمول سارا

برنامهٔ تحت وب محلی با C# / .NET Framework 4.7.2. ظاهر در مرورگر است. **هیچ پنجرهٔ CMD باز نمی‌شود.**

موتور همان RuleTrace قبلی است: Sara DLLها در زمان اجرا از `dll10` لود می‌شوند و VB فرمول بازنویسی/کامپایل نمی‌شود.

## بیلد آسان (ویندوز)

۱. فایل `RuleTrace.sln` را با **Visual Studio 2019 یا 2022** باز کنید  
۲. `F5` بزنید  

مرورگر روی `http://127.0.0.1:17880/` باز می‌شود. یک پنجرهٔ کوچک میزبان هم می‌ماند تا برنامه را ببندید.

اگر VS ندارید، در PowerShell (نه CMD):

```powershell
cd مسیر\پوشه\پروژه
Set-ExecutionPolicy -Scope Process Bypass
.\Build.ps1
```

`build.cmd` حذف شده تا CMD درگیر نشود.

فرم قدیمی WinForms فقط با این سوئیچ می‌آید: `RuleTrace.exe --desktop`

## استفاده

1. پوشه DLL: `Desktop\dll10` (دکمه «پیدا کردن خودکار DLL»)
2. تست اتصال دیتابیس — باید RuleEngine (`DbRuleEngein`)، Sara (`Sara8M03`) و Document (`DbRuleEngeinDocument`) با یوزر `debugger` OK باشند. جداول نام‌دار ضابطه در Sara پروب می‌شوند.
3. NidWorkItem یا کد نوسازی → جستجو → NidProc پر می‌شود
4. NidWorkItem `300002275` (پروانه تجدید بنا) → جستجو → NidProc پر می‌شود
5. دکمه طلایی **اجرا** — اول تیک بزنید کدام فرم را دیباگ می‌کنید (ضابطه / صلح / چیدمان / تحلیل / توافق / کمیسیون / درآمد). مثل باز کردن همان فرم در سارا؛ فقط همان جداول خوانده می‌شوند.
6. ملک می‌تواند صلح، توافق یا کمیسیون نداشته باشد. Active خالی یعنی این درخواست صلح ندارد — Member 1296 را عوض نکنید.
7. کلید زمین: `NidNosaziCode` → `Base_Info.NidBase`. توافق و صلح روی `Building='0'`. تحلیل در `AnalysisBuilding` (NidBase). چیدمان در `Base_Using` / `Base_Front`.

کلاس‌ها به هم وصل‌اند ولی **اجرا آن‌ها را با هم بار نمی‌کند.** فقط فرم‌های تیک‌خورده.

این برنامه `dbo.Member` را نمی‌نویسد و VB را کامپایل/بازنویسی نمی‌کند.

## فایل‌ها

```
RuleTrace.sln          ← F5 در Visual Studio
Program.cs             ← میزبان وب (پیش‌فرض) یا --desktop
WebUi.html             ← ظاهر فارسی RTL
WebHost.cs / WebApp.cs ← HttpListener روی 127.0.0.1
FormulaEngine.cs       ← موتور Sara با reflection
ChidmanAnalyzer.cs     ← تحلیل ایستای چیدمان ۱۲۸۸
PermitScopes.cs        ← انتخاب فرم (ضابطه/صلح/چیدمان/تحلیل/توافق/کمیسیون/درآمد)
PermitSteps.cs         ← طبقه‌بندی هر فرم مستقل
PermitPipeline.cs      ← مسیر پنج‌مرحله‌ای پروانه
SolhNidDebug.cs        ← مرحله صلح (ابزار بیشتر)
ZabetehCase.cs         ← جداول نام‌دار Sara (ضابطه + زمین + صلح + تحلیل + توافق)
RuleDocs.cs            ← مستند کلی MemberDocument + جداول دیگر DbRuleEngeinDocument
```

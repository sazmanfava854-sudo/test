# RuleTrace — جایگزین logfilefj بدون UI سارا

دیباگ فعلی سارا: تابع `Logfilefj` با UserGuid، بعد `logfilefj("نام", مقدار)` در فرمول، بعد باز کردن فرم (مثلاً ضابطه) تا همه AddError در پنجره پیغام بیاید.

**هدف RuleTrace:** همان فرآیند بدون باز شدن UI سارا. فرم را تیک بزنید، اجرا کنید، موس را روی هر خط نگه دارید تا مقدار متغیر مثل دیباگرهای امروزی دیده شود.

VB بازنویسی/کامپایل نمی‌شود. `dbo.Member` نوشته نمی‌شود.

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
5. فرم را تیک بزنید (ضابطه / صلح / …) — معادل باز کردن همان فرم در سارا
6. **اجرا** — کد Member همان فرم بار می‌شود؛ `logfilefj("نام", مقدار)` به خط‌ها وصل می‌شود
7. تب «کد Member» — موس را روی خط نگه دارید تا مقدار دیده شود (سبز = مقدار هست، زرد = probe بدون اجرا)

اگر Instanc بعد از RunRule خالی باشد، XmlBody از dbo.Member به `ClsFunction.Body` تزریق می‌شود و موتور Sara کامپایل می‌کند (نه ToString1، نه vbc چسبانده). **ClearCache را تیک نزنید.** UI سارا لازم نیست. RuleTrace VB را بازنویسی نمی‌کند.

`logfilefj` همان `Info8.AddError(Warning, A, B)` است؛ بعد از اجرای زنده، B در tooltip می‌آید. Member 1296 را عوض نکنید.

این برنامه `dbo.Member` را نمی‌نویسد و VB را کامپایل/بازنویسی نمی‌کند.

## فایل‌ها

```
RuleTrace.sln          ← F5 در Visual Studio
Program.cs             ← میزبان وب (پیش‌فرض) یا --desktop
WebUi.html             ← ظاهر فارسی RTL
WebHost.cs / WebApp.cs ← HttpListener روی 127.0.0.1
FormulaEngine.cs       ← موتور Sara با reflection
ChidmanAnalyzer.cs     ← تحلیل ایستای چیدمان ۱۲۸۸
PermitScopes.cs        ← تیک فرم (معادل باز کردن فرم سارا)
HoverDebug.cs          ← parse logfilefj + hover مقادیر
PermitSteps.cs         ← طبقه‌بندی هر فرم مستقل
PermitPipeline.cs      ← مسیر پنج‌مرحله‌ای پروانه
SolhNidDebug.cs        ← مرحله صلح (ابزار بیشتر)
ZabetehCase.cs         ← جداول نام‌دار Sara (ضابطه + زمین + صلح + تحلیل + توافق)
RuleDocs.cs            ← مستند کلی MemberDocument + جداول دیگر DbRuleEngeinDocument
```

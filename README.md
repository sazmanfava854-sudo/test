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
4. فرمول `Solh`، Watch `Calc_Chandganeh`
5. دکمه طلایی **اجرا** — ReCompile خاموش، **پاک کردن Cache خاموش**. اگر Instanc ساخته نشود (خروج ۲)، همان اجرا ضابطه را با join `NidNosaziCode` می‌خواند و در خلاصهٔ کپی می‌گذارد (دیگر فقط BodyLen=0 نیست). NidProc باید پر باشد.
6. **دیباگ پروانه این Nid** — CRUD=Read. همهٔ پرونده‌های پروانه همین مسیر را دارند: ضابطه (Rule 336 / ZabetehConvert 342) → صلح (Solh 344 / Tavafogh 345) → تحلیل (Takhalofat 338) → کمیسیون ماده ۱۰۰ (Commission 340 / CommissionFine 335) → درآمد (Income 337). پروندهٔ نمونه WorkItem `300002275` (پروانه تجدید بنا). اگر ردیف `Zabeteh` با `NidNosaziCode` باشد ولی `ActiveNidZabeteh` خالی باشد، ضابطه هست و اعلام نشده؛ L270 درست می‌ایستد — Member 1296 را عوض نکنید. WorkItem `5298603` بررسی نمی‌شود.
7. **مستند کلی** — همان کوئری SSMS بدون فیلتر Member:
   `SELECT TOP (1000) DocId, Title, NidMember, MemberDocument, LastEditOn, Sort, ParentDocId, UserName FROM [DbRuleEngeinDocument].[dbo].[MemberDocument]`
   با یوزر `debugger`. جداول **دیگر** همان دیتابیس هم لیست و peek می‌شوند. عنوان/متن مستند برای نام جداول ضابطه (`Zabeteh`، `CI_PlanType`، …) جستجو می‌شود.
8. **بررسی فرمول از DB** و تب تاریخچه — لاگ `NidHistory` / `MemberHistory`

کلاس‌ها به هم وصل‌اند. Member چیدمان `1288` در `ZabetehConvert` (342) است نه Solh/344. با انتخاب Solh این کلاس‌ها با هم خوانده می‌شوند: Rule 336، ZabetehConvert 342، Solh 344، Tavafogh 345، Global 432.

اگر Instanc نبود (کد خروج ۲): این طبیعی است. منبع عیب‌یابی `dbo.Member` و لاگ `NidHistory` است، نه DLL. دکمه **اجرا** هنوز همان RunRule موتور است؛ **بررسی فرمول از DB** / **تحلیل چیدمان ۱۲۸۸** متن و تاریخچه را می‌خوانند.

## معماری اجرا (بدون تغییر نسبت به v21c)

1. `ClsCommon.RunRule`
2. اگر `Instanc` ساخته شد → `SetMyInfo` + `Run`
3. اگر نه → DLL از قبل کامپایل‌شده در Cache / `dll10`
4. اگر هیچ‌کدام نبود → تحلیل ایستای Member 1288 بدون کامپایل VB

این برنامه `dbo.Member` را نمی‌نویسد.

## فایل‌ها

```
RuleTrace.sln          ← F5 در Visual Studio
Program.cs             ← میزبان وب (پیش‌فرض) یا --desktop
WebUi.html             ← ظاهر فارسی RTL
WebHost.cs / WebApp.cs ← HttpListener روی 127.0.0.1
FormulaEngine.cs       ← موتور Sara با reflection
ChidmanAnalyzer.cs     ← تحلیل ایستای چیدمان ۱۲۸۸
PermitPipeline.cs      ← مسیر پنج‌مرحله‌ای پروانه
SolhNidDebug.cs        ← مرحله صلح داخل همان مسیر (Read-only)
ZabetehCase.cs         ← جداول ضابطه Sara
RuleDocs.cs            ← مستند کلی MemberDocument + جداول دیگر DbRuleEngeinDocument
```

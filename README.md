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
2. تست اتصال دیتابیس — باید RuleEngine و Sara هر دو OK باشند
3. NidWorkItem یا کد نوسازی → جستجو → NidProc پر می‌شود
4. فرمول `Solh`، Watch `Calc_Chandganeh`
5. دکمه طلایی **اجرا** — ReCompile خاموش، **پاک کردن Cache خاموش** (اگر تیک باشد DLL کامپایل‌شده Solh حذف می‌شود و Exit 2 می‌گیرید)
6. **بررسی فرمول از DB** و تب تاریخچه — لاگ `NidHistory` / `MemberHistory` (ستون `Body` از نوع `image` است؛ لیست فقط اندازه می‌گیرد)

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
```

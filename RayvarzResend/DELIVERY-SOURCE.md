# تحویل سورس (غیرپابلیش) — نسخه آخر

اجرا با `dotnet run` از همین پوشه — بدون exe publish.

ورود یکپارچه (شیماس): `Auth:Shimas` در `appsettings.json`  
- ClientId / LKey: `19cf3C33`  
- ClientSecret: `D2fbf` (فقط سمت سرور؛ در URL مرورگر نمی‌رود)  
- callback: `https://city.mashhad.ir:5065/auth/callback`  
- هر کاربر در تب مدیریت باید **دامین** داشته باشد (مثلاً `hoseine-sh`) تا با لاگین کلی یکی شود.

تنظیمات: فقط `RayvarzResend.Web\appsettings.json`

اگر `dotnet run` خطای JSON داد، بعد از ویرایش ConnectionStrings ویرگول بعد از `}` جا مانده است. بخش اتصال الان انتهای فایل است — فقط Server و Password را عوض کنید؛ `"` داخل رمز را به‌صورت `\"` بنویسید.

## اجرا

```powershell
cd RayvarzResend\RayvarzResend.Web
dotnet run
```

## تغییرات این نسخه

| مورد | رفتار |
|------|--------|
| منبع | جدول منطقه → کد شعبه / کد منبع (بدون FinancialAssistant در صفحه) |
| ورود | لاگین یکپارچه با ClientId `19cf3C33`؛ هدایت به login.mashhad.ir |
| تهاتر | فقط فیش وارد/انتخاب‌شده؛ جفت ۱۵۷/۱۵۸ خودکار ارسال نمی‌شود |
| واسط Sara | پس از ارسال موفق: `Accounting_DocHeader` و `Accounting_DocDetails` |
| نتیجه ارسال | کارت داخل برنامه؛ بدون جفت مرجع / جزئیات فیش / force=true |
| پیش‌نمایش XML | حذف |

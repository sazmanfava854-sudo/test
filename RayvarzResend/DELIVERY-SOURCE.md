# تحویل سورس (غیرپابلیش) — نسخه آخر

اجرا با `dotnet run` از همین پوشه — بدون exe publish.

ورود کاربران: `https://city.mashhad.ir:5065/login.html`  
صفحه ورود: عنوان «ورود به دستیار مالی» + کد ملی و رمز عبور (بدون جملهٔ «با کد ملی وارد شود»).

تنظیمات: فقط `RayvarzResend.Web\appsettings.json`

## اجرا

```powershell
cd RayvarzResend\RayvarzResend.Web
dotnet run
```

## تغییرات این نسخه

| مورد | رفتار |
|------|--------|
| منبع | جدول منطقه → کد شعبه / کد منبع (بدون FinancialAssistant در صفحه) |
| ورود | بدون متن «با کد ملی وارد شود» |
| تهاتر | فقط فیش وارد/انتخاب‌شده؛ جفت ۱۵۷/۱۵۸ خودکار ارسال نمی‌شود |
| واسط Sara | پس از ارسال موفق: `Accounting_DocHeader` و `Accounting_DocDetails` |
| نتیجه ارسال | کارت داخل برنامه؛ بدون جفت مرجع / جزئیات فیش / force=true |
| پیش‌نمایش XML | حذف |

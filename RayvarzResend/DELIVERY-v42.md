# تحویل — RayvarzResend **v42**

Tag: `rayvarzresend-tahator-dtl-v42` — `releaseVersion: 42`

[RayvarzResend-42.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-42.zip)

## تغییرات نسبت به v41

- **تب «تغییر تاریخ چک»:** جستجو با `TrackingNo` در `Installment_List`، تغییر `PaymentDate`، کامنت با کاربر و مورخ.
- **مدیریت کاربران:** دسترسی جدا **تغییر تاریخ چک** (گروه / شخصی) — مستقل از چک خزانه.
- **تغییر تاریخ فیش:** کامنت فقط برای فیلدهایی که واقعاً عوض شده‌اند (مثلاً وضعیت با مقدار قبلی).
- **رمز عبور:** ریست ادمین در مدیریت کاربران اصلاح شد؛ تغییر رمز توسط خود کاربر بدون رمز فعلی؛ دکمه نمایش رمز (👁).
- **دیتابیس AppAuth:** migration `database/11_AppUser_InstallmentCheckDate.sql` برای مجوز تب تاریخ چک.

شامل قابلیت‌های v41 (چک رایورز + `Accounting_DocHeader` قبل از ارسال، ثبت Header/Details پس از SOAP).

## استقرار

1. Zip را باز کنید → `RayvarzResend\`
2. `appsettings.json` را تنظیم کنید.
3. در صورت استفاده از SQL برای کاربران: اسکریپت‌های `database/11_AppUser_InstallmentCheckDate.sql` (و در صورت نیاز قبلی‌ها) روی AppAuth.
4. `start.bat` یا `RayvarzResend.Web.exe`
5. `GET /api/config` → `releaseVersion: 42`

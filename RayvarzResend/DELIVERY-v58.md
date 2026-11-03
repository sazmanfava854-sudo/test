# تحویل — RayvarzResend **v58**

Tag: `rayvarzresend-tahator-dtl-v58` — `releaseVersion: 58`

**[دانلود RayvarzResend-58.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend-58.zip)**

**[دانلود راهنمای کاربر (Word — v58)](https://github.com/sazmanfava854-sudo/test/raw/cursor/tahator-income-dtl-84f2/RayvarzResend/docs/RayvarzResend-UserManual-fa-v58.docx)** — بدون «مدیریت کاربران» و «شاهکار»؛ فصل‌های ورود/رابط عمومی حذف شده.

## تب خزانه — چند فیش یک شماره سند (مثلاً 502267)

- کد پیگیری در گرید از **دیتابیس** (`TrackingNo` یا در صورت خالی بودن `CheckRahgiriCode`) — نه از اکسل.
- فیش **دوم** همان سند: مبلغ/تاریخ اکسل دیگر مانع انتخاب نیست؛ پیام راهنما: «فیش دیگر همان شماره سند…».
- اگر `TrackingNo` خالی باشد ولی ردیف در `Installment_List` شناسه دارد → انتخاب و UPDATE با `NidInsList`.
- رنگ گروه فقط برای سندهای با **بیش از یک** فیش در گرید.

شامل v57 و قبل. **Ctrl+F5** — `app.js?v=114`.

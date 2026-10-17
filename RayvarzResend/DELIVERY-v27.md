# تحویل — RayvarzResend **نسخه آخر** (v27 — PR #115)

شاخه: [`cursor/unsent-search-optional-date-4c4a`](https://github.com/sazmanfava854-sudo/test/tree/cursor/unsent-search-optional-date-4c4a) — SendGate پس از تغییر Rule Engine، جستجوی فیش‌های ارسال‌نشده بدون بازه تاریخ اجباری، اصلاح اکسل تهاتر (شناسه قبض=پرداخت=شماره فیش)، به‌همراه v26 (اکسل mixed، epay، Accounting_Doc، تهاتر).

## دریافت

| روش | مسیر |
|-----|------|
| **Tag نسخه** | `rayvarzresend-pr115-v27` |
| Zip publish | `RayvarzResend-27.zip` |
| شاخه | [cursor/unsent-search-optional-date-4c4a](https://github.com/sazmanfava854-sudo/test/tree/cursor/unsent-search-optional-date-4c4a) |

### لینک مستقیم Zip

[دانلود RayvarzResend-27.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/unsent-search-optional-date-4c4a/RayvarzResend-27.zip)

Tag ثابت:

[RayvarzResend-27.zip روی tag v27](https://github.com/sazmanfava854-sudo/test/raw/rayvarzresend-pr115-v27/RayvarzResend-27.zip)

### اجرا روی سرور ویندوز

Zip فقط **یک پوشه** دارد: `RayvarzResend\`

| فایل | کاربرد |
|------|--------|
| `RayvarzResend\start.bat` | اجرا (پورت 5088) |
| `RayvarzResend\RayvarzResend.Web.exe` | exe (نیاز به .NET 8 Runtime روی سرور) |
| `RayvarzResend\appsettings.json` | **تنها** فایل تنظیمات |

`Rayvarz:DryRun=false` و `AccountingDoc:DryRun=false` داخل همان `appsettings.json`.

پس از اجرا: `GET /api/config` → `releaseVersion: 27` ، `accountingDoc.dryRun: false`.

ساخت Zip: `bash RayvarzResend/scripts/build-release-zip.sh`

## خلاصه تغییرات نسبت به v26

| موضوع |
|--------|
| **SendGate:** پس از تشخیص تغییر Rule روی Member 1388، ارسال به رایورز تا تأیید ادمین مسدود (`409`) |
| پیام کاربر عادی: `باید برنامه به‌روزرسانی شود` — پنل مدیریت: `نیاز به به روز رسانی هست` |
| **ارسال‌نشده:** بازه تاریخ اجباری نیست؛ حداقل یک فیلتر دیگر یا بازه کامل |
| **اکسل جمعی تهاتر:** ردیف با شناسه قبض=پرداخت (و شماره فیش) بدون خطای جابه‌جایی ستون |
| تمپلیت اکسل: ستون اختیاری **شماره فیش** |

## تست

```bash
cd RayvarzResend && dotnet test   # 573+
```

## نسخه قبلی

- [DELIVERY-v26.md](DELIVERY-v26.md) — tag `rayvarzresend-unified-excel-epay-v26`

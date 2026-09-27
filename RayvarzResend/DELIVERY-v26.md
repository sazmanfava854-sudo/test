# تحویل — RayvarzResend **نسخه آخر** (v26)

شاخه: `cursor/unified-excel-epay-release-ffcb` — اکسل دسته‌ای (قبض/پرداخت بدون ستون نوع)، جستجوی mixed Income سپس Duty، دروازه epay قبل از ارسال، به‌همراه Accounting_Doc و تهاتر v25.

## دریافت

| روش | مسیر |
|-----|------|
| **Tag نسخه** | `rayvarzresend-unified-excel-epay-v26` |
| Zip publish | `RayvarzResend-26.zip` |
| Zip سورس | `RayvarzResend-source.zip` |
| شاخه | [cursor/unified-excel-epay-release-ffcb](https://github.com/sazmanfava854-sudo/test/tree/cursor/unified-excel-epay-release-ffcb) |

### لینک مستقیم Zip (پس از push/tag)

```
https://github.com/sazmanfava854-sudo/test/raw/rayvarzresend-unified-excel-epay-v26/RayvarzResend-26.zip
```

### اجرا روی سرور ویندوز

Zip فقط **یک پوشه** دارد: `RayvarzResend\`

| فایل | کاربرد |
|------|--------|
| `RayvarzResend\start.bat` | اجرا (پورت 5088) |
| `RayvarzResend\RayvarzResend.Web.exe` | exe (نیاز به .NET 8 Runtime روی سرور) |
| `RayvarzResend\appsettings.json` | **تنها** فایل تنظیمات |

`appsettings.Production.json` خوانده نمی‌شود — اگر از قبل کنار exe هست حذف کنید.

`Rayvarz:DryRun=false` و `AccountingDoc:DryRun=false` داخل همان `appsettings.json`.

پس از اجرا: `GET /api/config` → `releaseVersion: 26` ، `accountingDoc.dryRun: false`.

ساخت Zip: `bash RayvarzResend/scripts/build-release-zip.sh`

`GET /api/config` → `releaseVersion: 26` ، `releaseLabel: نسخه آخر`

## خلاصه تغییرات نسبت به v25

| موضوع |
|--------|
| تب ارسال دسته‌ای: ورود از اکسل (۲ ستون قبض/پرداخت)، تمپلیت، کتابخانه xlsx محلی |
| جستجو: ابتدا `Income_Fiche`، سپس `Duty_Fiche`؛ تعارض هر دو → لاگ، بدون افزودن به گرید |
| قبل از ارسال تک/دسته‌ای: بررسی حضور فیش در epay (`EpayFichePresenceChecker`) |
| بهینه‌سازی SQL batch lookup (کاهش timeout اکسل) |
| UI گرید دسته‌ای، وضعیت «فایل در حال بررسی است…»، لاگ نتیجه ارسال |

## تست

```bash
cd RayvarzResend && dotnet test
```

## نسخه قبلی

- [DELIVERY-v25.md](DELIVERY-v25.md) — tag `rayvarzresend-noskhe-akhar`

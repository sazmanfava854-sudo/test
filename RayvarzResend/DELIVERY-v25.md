# تحویل — RayvarzResend **نسخه آخر** (v25)

شاخه: `cursor/tahator-accounting-doc-ffcb` — جمع‌بندی Accounting_Doc، تهاتر تک‌فیش، تشخیص جفت با Payable، poll `incmdocsys`.

## دریافت

| روش | مسیر |
|-----|------|
| **لیبل / Tag نسخه آخر** | `rayvarzresend-noskhe-akhar` |
| Zip publish | `RayvarzResend-25.zip` |
| GitHub Release | [releases/tag/rayvarzresend-noskhe-akhar](https://github.com/sazmanfava854-sudo/test/releases/tag/rayvarzresend-noskhe-akhar) |
| شاخه | [cursor/tahator-accounting-doc-ffcb](https://github.com/sazmanfava854-sudo/test/tree/cursor/tahator-accounting-doc-ffcb) |

### لینک مستقیم Zip

```
https://github.com/sazmanfava854-sudo/test/releases/download/rayvarzresend-noskhe-akhar/RayvarzResend-25.zip
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

پس از اجرا: `GET /api/config` → `releaseVersion: 25` ، `accountingDoc.dryRun: false`.

ساخت Zip: `bash RayvarzResend/scripts/build-release-zip.sh`

`GET /api/config` → `releaseVersion: 25` ، `releaseLabel: نسخه آخر`

## خلاصه تغییرات نسبت به v24

| موضوع |
|--------|
| Poll `incmdocsys` + فیلتر DocTyp تهاتر برای ثبت واسط |
| ارسال **فقط** فیش درخواستی (بدون ارسال خودکار جفت ۱۵۸) |
| جفت تهاتر: اولویت **Payable** یکسان سپس NidExportation |
| `DocRow=1` برای سربرگ حسابداری تهاتر |

## تست

```bash
cd RayvarzResend && dotnet test   # 411
```

## نسخه قبلی

- [DELIVERY-v24.md](DELIVERY-v24.md) — tag `rayvarzresend-tahator-accounting-v24`

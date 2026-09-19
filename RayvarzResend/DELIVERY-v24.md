# تحویل — RayvarzResend نسخه ۲۴

نسخه **v24** بر پایه **bank-inquiry 166a** + **ثبت واسط Sara** پس از ارسال موفق به رایورز.

## دریافت

| روش | مسیر |
|-----|------|
| Zip سورس + publish ویندوز | `RayvarzResend-24.zip` (ریشه مخزن) |
| Git tag | `rayvarzresend-tahator-accounting-v24` |
| شاخه | `cursor/tahator-accounting-doc-ffcb` |
| GitHub Release | [releases/tag/rayvarzresend-tahator-accounting-v24](https://github.com/sazmanfava854-sudo/test/releases/tag/rayvarzresend-tahator-accounting-v24) |

`GET /api/config` → `releaseVersion: 24`

### لینک مستقیم Zip (پس از push/tag)

```
https://github.com/sazmanfava854-sudo/test/raw/rayvarzresend-tahator-accounting-v24/RayvarzResend-24.zip
```

### اجرای publish آماده (ویندوز سرور)

داخل Zip: `RayvarzResend/publish/win-x64/` — نیاز به **.NET 8 Runtime** روی سرور.

```powershell
cd RayvarzResend\publish\win-x64
.\RayvarzResend.Web.exe
```

یا سورس: `dotnet publish` / `publish-and-run.ps1` طبق README.

## تغییرات v24

| موضوع | فایل‌ها |
|--------|---------|
| INSERT `Accounting_DocHeader` / `Accounting_DocDetails` پس از تأیید `incmdocsys` | `AccountingDocWriter.cs`, `AccountingDocRowBuilder.cs` |
| تهاتر (۱۵۷/۱۵۸) | `TahatorResendService.cs` |
| درآمد + نوسازی + صنفی | `FicheSendService.cs` |
| متادیتای `AccountingNo` از incmdocsys | `FicheRepository.GetRayvarzDocMetaAsync` |
| خطا | خواندن `Accounting_DocNotSent.Comment` (همان قبل) |

پیکربندی: `Rayvarz:DryRun=false` و در صورت نیاز `AccountingDoc:DryRun=false`.

## تست

```bash
cd RayvarzResend
dotnet test   # 410 تست
```

## نسخه‌های قبلی

- v23 — [`DELIVERY-v23.md`](DELIVERY-v23.md)
- bank-inquiry — tag `rayvarzresend-bank-inquiry-166a`

# تحویل — RayvarzResend **نسخه آخر** (v28 — PR #115)

شاخه: [`cursor/unsent-search-optional-date-4c4a`](https://github.com/sazmanfava854-sudo/test/tree/cursor/unsent-search-optional-date-4c4a) — **بهینه‌سازی ورود از اکسل** (جستجوی سریع `CROSS APPLY`)، به‌همراه v27 (SendGate، ارسال‌نشده، اکسل تهاتر).

## دریافت

| روش | مسیر |
|-----|------|
| **Tag نسخه** | `rayvarzresend-pr115-v28` |
| Zip publish | `RayvarzResend-28.zip` |
| شاخه | [cursor/unsent-search-optional-date-4c4a](https://github.com/sazmanfava854-sudo/test/tree/cursor/unsent-search-optional-date-4c4a) |

### لینک مستقیم Zip

[دانلود RayvarzResend-28.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/unsent-search-optional-date-4c4a/RayvarzResend-28.zip)

Tag ثابت:

[RayvarzResend-28.zip روی tag](https://github.com/sazmanfava854-sudo/test/raw/rayvarzresend-pr115-v28/RayvarzResend-28.zip)

### اجرا روی سرور ویندوز

Zip فقط **یک پوشه** دارد: `RayvarzResend\` — `start.bat` ، یک `appsettings.json` ، پورت **5088**.

پس از اجرا: `GET /api/config` → `releaseVersion: 28`.

ساخت Zip: `bash RayvarzResend/scripts/build-release-zip.sh`

## خلاصه تغییرات نسبت به v27

| موضوع |
|--------|
| ورود اکسل ارسال‌نشده: بدون اسکن کامل `Income_Fiche` / `Duty_Fiche` — `CROSS APPLY` به‌ازای هر ردیف |
| مسیر جدا برای تهاتر (`bill=pay`) و قبض/پرداخت متفاوت |
| جستجوی Duty موازی برای کاهش timeout |

## نسخه قبلی

- [DELIVERY-v27.md](DELIVERY-v27.md) — tag `rayvarzresend-pr115-v27`

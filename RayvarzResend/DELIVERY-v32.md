# تحویل — RayvarzResend **نسخه آخر** (v32 — PR #115)

شاخه: [`cursor/unsent-search-optional-date-4c4a`](https://github.com/sazmanfava854-sudo/test/tree/cursor/unsent-search-optional-date-4c4a)

## دریافت

[دانلود RayvarzResend-32.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/unsent-search-optional-date-4c4a/RayvarzResend-32.zip)

Tag: `rayvarzresend-pr115-v32`

`GET /api/config` → `releaseVersion: 32`

## نسبت به v31

شماره فیش، شناسه قبض و شناسه پرداخت اگر با رقم فارسی یا عربی وارد شوند، قبل از جستجو به رقم انگلیسی تبدیل می‌شوند. این تبدیل در همه تب‌ها (ارسال تکی، ارسال‌نشده، اکسل، تغییر تاریخ، خدمات الکترونیک، اقساط، شاهکار) اعمال می‌شود. مثال: `۰۵۰۸۳۳۵۳۳۲۴۲` همان `050833533242` جستجو می‌شود.

در ورود اکسل، اگر فیش تهاتر پیدا شده در رایورز مبلغ منفی داشته باشد، گرید فیش جفت ارسال‌نشده را نشان می‌دهد. نمونه: `050833533242` با Val منفی جای خود را به `050833533243` می‌دهد.

## نسخه قبلی

- [DELIVERY-v31.md](DELIVERY-v31.md)

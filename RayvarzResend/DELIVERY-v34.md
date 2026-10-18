# تحویل — RayvarzResend **v34** (PR #116 — مرکز هزینه)

شاخه: [`cursor/cost-center-pr116-9665`](https://github.com/sazmanfava854-sudo/test/tree/cursor/cost-center-pr116-9665)

Tag: `rayvarzresend-pr116-v34`

`GET /api/config` → `releaseVersion: 34`

## دریافت

[دانلود RayvarzResend-34.zip](https://github.com/sazmanfava854-sudo/test/raw/cursor/cost-center-pr116-9665/RayvarzResend-34.zip)

## تغییرات نسبت به v26

- ماژول **مرکز هزینه** در صفحه مدیریت و تب اصلی
- جستجو با **شماره فیش** (همان منطق صفر پیشرو و چند فیش با ویرگول)
- گرید نتایج با انتخاب فیش
- فیلدهای **مرکز یک** (`Deposit`) و **مرکز دو** (`CreditorPapers`)
- `POST /api/cost-center/search` و `POST /api/cost-center/update`
- دسترسی گروهی/شخصی `CanAccessCostCenter` — اسکریپت `database/13_AppUserGroup_CostCenter.sql`

## نسخه قبلی (مستندات)

- [DELIVERY-v26.md](DELIVERY-v26.md)

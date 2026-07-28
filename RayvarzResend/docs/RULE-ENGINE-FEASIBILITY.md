# امکان‌سنجی: یک منبع حقیقت از `DbRuleEngein.dbo.Member` به‌جای کپی C#

## هدف

به‌جای نگهداری **دومین نسخه** از منطق (VB در شهرسازی + C# در RayvarzResend)، قوانین از دیتابیس خوانده شوند و خروجی **همان ورودی SaveDocument** ساخته شود. نمونه جدول:

```sql
SELECT NidClass, NidMember, FromDate, ToDate, Body, EnumType, isActive,
       Version, VersionDateTime, XmlBody, EncryptXmlBody
FROM DbRuleEngein.dbo.Member
WHERE NidMember = 1388;  -- مثال: نوسازی / ارسال رایورز
```

## پاسخ کوتاه

| بخش | امکان‌پذیری |
|-----|-------------|
| **خواندن** نسخه فعال قانون از DB (با `Version` / `FromDate` / `isActive`) | بله — معمول |
| **اجرای همان منطق VB** فقط با خواندن `XmlBody` در .NET 8 بدون موتور شهرسازی | **معمولاً خیر** (مگر XmlBody DSL باشد، نه کد کامل) |
| **یک منبع حقیقت بدون دو نسخه** در عمل | **بله**، با یکی از الگوهای زیر |

## `XmlBody` معمولاً چیست؟

در سیستم‌های قانون‌گذاری شهرسازی اغلب یکی از این حالت‌هاست:

1. **متادیتا / گراف قوانین** (شرط، فیلد، فرمول) که موتور **DbRuleEngein** در همان پروسه Sara تفسیر می‌کند — نه متن VB قابل `dotnet run`.
2. **بدنه VB** (`Body`) + نسخه XML برای UI یا diff — وابسته به `Info8`, `BIZ.Communication`, `DutyFicheResultList`, …
3. **رمزنگاری** (`EncryptXmlBody`) — نیاز به همان کلید/سرویس decrypt شهرسازی.

تا وقتی نمونه واقعی `XmlBody` (و در صورت نیاز decrypt) برای `NidMember=1388` دیده نشود، نمی‌توان گفت «فقط parse XML و SOAP بساز»؛ باید **یک بار** روی سرور 232 استخراج و طبقه‌بندی شود.

## الگوهای معماری (از ساده به نزدیک به «یک نسخه»)

### ۱) فراخوانی موتور موجود شهرسازی (توصیه برای کم‌ریسک)

- RayvarzResend فقط: بارگذاری فیش، پارامترها، **فراخوانی API/سرویس داخلی** که همان `ClsAccounting` / `Nosazi()` را اجرا می‌کند.
- خروجی: `ClsRayvarzMessageContract` یا XML نهایی.
- **یک منبع حقیقت:** VB + Rule Engine همان Sara؛ بدون پورت C#.
- نیاز: endpoint امن (VPN)، قرارداد DTO، timeout.

### ۲) Host کردن همان اسمبلی‌های Rule Engine + Communication

- RayvarzResend به‌عنوان **فرآیند جانبی** با همان DLLهای `BIZ.*` / RuleEngein که روی سرور شهرسازی نصب است.
- از DB فقط **شناسه Member + Version** می‌گیرید؛ اجرا با runtime شهرسازی.
- چالش: نسخه DLL، GAC، `Info8` context، لایسنس، 32/64 bit.

### ۳) سرویس واسط «محاسبه سند»

- یک Windows Service / API کوچک **روی همان سرور Sara** که:
  - `NidMember` + `NidFiche` (+ تاریخ مؤثر) را می‌گیرد
  - Rule Engine را صدا می‌زند
  - payload رایورز را برمی‌گرداند
- RayvarzResend فقط UI + SOAP send + dry-run.

### ۴) تفسیر اعلانی `XmlBody` در .NET (فقط اگر DSL باشد)

- اگر `XmlBody` واقعاً گراف قوانین مستقل از VB باشد، می‌توان **مفسر** جدا در C# نوشت.
- **ریسک:** هر تغییر در فرمت XML موتور قدیم باید همزمان در مفسر جدید منعکس شود — دوباره دو نسخه مگر موتور یکی باشد.

### ۵) ادامه C# ثابت (baseline v16)

- منطق از `nosazo.vb` کپی شده؛ با تغییر VB در شهرسازی **دوباره drift** می‌کنید.
- برای resend اضطراری خوب است؛ برای «همیشه یک منبع» کافی نیست.

## انتخاب نسخه قانون در زمان

```sql
-- الگوی پیشنهادی (ستون‌ها را با اسکیمای واقعی تطبیق دهید)
SELECT TOP 1 XmlBody, Version, VersionDateTime
FROM DbRuleEngein.dbo.Member
WHERE NidMember = @nid
  AND isActive = 1
  AND @asOf BETWEEN FromDate AND ISNULL(ToDate, '9999-12-31')
ORDER BY Version DESC, VersionDateTime DESC;
```

برای **فیش تاریخ‌دار** باید «تاریخ مؤثر» (پرداخت / صدور) با `FromDate`/`ToDate` Member هم‌تراز شود.

## جمع‌بندی برای تصمیم شما

- **خواندن کد از DB:** بله.
- **جایگزینی کامل پورت C# با XmlBody خام، بدون موتور Sara:** در بیشتر موارد **عملی نیست** اگر XmlBody وابسته به VB و `Info8` باشد.
- **یک نسخه قانون، بدون دو پیاده‌سازی:** **بله** اگر RayvarzResend به‌جای «بازنویسی»، **همان اجرای قانون شهرسازی** (۱ یا ۲ یا ۳) را صدا بزند؛ DB فقط انتخاب Member/Version است.

## گام‌های پیشنهادی بعدی (کشف)

1. Export یا کوئری `XmlBody` برای **`NidMember = 1388` فقط** (نسخه فعال).
2. مشخص کردن: Sara هنگام ارسال رایورز همان Member را با `NidClass=360` / `1388` صدا می‌زند.
3. POC: SaraBridge با `nidMember: 1388` ثابت.

**توجه:** Memberهای دیگر DbRuleEngein خارج از scope ارسال رایورز هستند.

Baseline قابل بازگشت: `BASELINE-v16.md` و tag `rayvarz-resend-baseline-v16`.

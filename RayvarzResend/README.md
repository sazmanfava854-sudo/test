# RayvarzResend — فرم تست ارسال مجدد به رایورز

**نسخه تحویل: ۱۵** (`rayvarz-resend-v15`)

| دریافت | آدرس |
|--------|------|
| Zip شاخه (همیشه آخرین commit) | `https://github.com/sazmanfava854-sudo/test/archive/refs/heads/rayvarz-resend.zip` |
| Zip تگ نسخه ۱۵ (ثابت) | `https://github.com/sazmanfava854-sudo/test/archive/refs/tags/rayvarz-resend-v15.zip` |

پس از unzip، در هدر فرم یا `GET /api/config` باید `releaseVersion: 15` ببینید.

فرم وب ساده برای تست ارسال فیش به وب‌سرویس رایورز (محیط تست).

## پیش‌نیاز

- .NET 8 SDK
- دسترسی به SQL Server (Sara8M03 + Ray_CityHall)
- شبکه داخلی / VPN برای وب‌سرویس تست

## راه‌اندازی

### 1) تنظیم connection string

فایل `RayvarzResend.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "Sara": "Server=SERVER;Database=Sara8M03;Trusted_Connection=True;TrustServerCertificate=True;",
  "Rayvarz": "Server=SERVER;Database=Ray_CityHall;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2) حالت DryRun (اولین تست)

```json
"Rayvarz": {
  "SourceSystemId": "11111",
  "DryRun": true
}
```

### 3) اجرا

```powershell
cd RayvarzResend\RayvarzResend.Web
dotnet run
```

## مپینگ BnkAcntNo = کد نوسازی

**`BnkAcntNo` در رایورز همان «کد نوسازی» است** (نه شماره حساب بانکی).

نمونه تأییدشده: `10-8-276-11-0-0-0` (فیش `101104/9881711`)

| نوع فیش | جدول | منبع کد نوسازی |
|---------|------|----------------|
| **درآمد** | `Income_Fiche` | `Base_NosaziCode` از join: `Income → Sh_RequestInfo` |
| **نوسازی/صنفی** | `Duty_Fiche` | `OtherFields` → `کد نوسازی` (XML) |

### کوئری دستی

**نوسازی/صنفی:**
```sql
SELECT OtherFields.value('(//ClsLog[Subject="کد نوسازي"]/Value)[1]', 'nvarchar(100)') AS BnkAcntNo
FROM dbo.Duty_Fiche WHERE FicheNo = N'101104/9881711';
-- نتیجه: 10-8-276-11-0-0-0
```

**درآمد:**
```sql
SELECT CAST(b.CI_City AS varchar) + '-' + CAST(b.District AS varchar) + '-' +
       CAST(b.Region AS varchar) + '-' + CAST(b.Block AS varchar) + '-' +
       CAST(b.House AS varchar) + '-' + CAST(b.Building AS varchar) + '-' +
       CAST(b.Apartment AS varchar) + '-' + CAST(b.Shop AS varchar) AS BnkAcntNo
FROM dbo.Income_Fiche f
JOIN dbo.Income i ON i.NidIncome = f.NidIncome
JOIN dbo.Sh_RequestInfo r ON r.NidProc = i.NidProc
JOIN dbo.Base_NosaziCode b ON b.NidNosaziCode = r.NidNosaziCode
WHERE f.FicheNo = @FicheNo;
```

## مپینگ سایر فیلدهای SOAP

| فیلد SOAP | منبع |
|-----------|------|
| TransactionId | پیش‌فرض **GUID جدید در هر ارسال** (`TransactionIdMode=newGuidPerSend`) تا خطای «تراکنش تکراری» نشود؛ برای رفتار سامانه اصلی: `nidFiche` |
| SourceId | `appsettings → SourceSystemId` (`11111`) |
| RowDocNo | `FicheNo` |
| Ref2 / Ref3 | `BillID` / `PaymentID` |
| Qty | `Payable` (مبلغ کل) |
| Val | مبلغ هر ردیف |
| Bank | `ConfirmBankCode` |
| تاریخ | **DocDate** / **ActDate** / **Due** هر کدام از ستون‌های فیش (و قابل ویرایش در فرم) — بدون مقدار ثابت در appsettings |

### DocDate / ActDate / Due (از فیش Sara8M03)

| SOAP | درآمد (`Income_Fiche`) | نوسازی/صنفی (`Duty_Fiche`) |
|------|------------------------|----------------------------|
| **DocDate** | `PaymentDate` → `BankPaymentDate` | `PrintDate` → `ExportDate` → `PaymentDate` → `BankPaymentDate` |
| **ActDate** / **RowDate** | `BankPaymentDate` → `PaymentDate` | `BankPaymentDate` → `PaymentDate` → `PrintDate` → `ExportDate` |
| **Due** / **RefRowDate** (ردیف) | `BankPaymentDate` → `PaymentDate` | همان **Due** از فیش |

پس از «دریافت فیش» سه فیلد تاریخ پر می‌شوند؛ برای هر فیش می‌توان جداگانه اصلاح کرد.

**نوسازی/صنفی:** منطق `Nosazi()` از سامانه شهرسازی (`DutyNosaziLogic.cs`):

- **آتش‌نشانی:** `SUM(F5,F0) − SUM(F5,F≠0)`
- **پسماند:** `SUM(F3,F0) − SUM(F3,F≠0)`
- **ارزش‌افزوده:** `SUM(F3,F16)`
- **ردیف اصلی (۲۰۰۳/…):** `Val = PayablePrice − آتش − پسماند − ارزش‌افزوده`
- **Qty:** `PayablePrice` در هر ردیف
- **تاریخ:** `DocDate`/`Due` = امروز شمسی؛ `ActDate`/`RowDate` = PaymentDate یا BankPaymentDate بر اساس وضعیت فیش
- **شعبه/Fund:** `DutyDistrictBranchResolver` از BillID/PaymentID (nosazo.vb)
| branch / Fund | منطقه فیش (`OtherFields → منطقه` برای نوسازی) |

## فیش‌های تست تأییدشده

| FicheNo | BnkAcntNo | branch | Fund |
|---------|-----------|--------|------|
| `101104/9881711` | `10-8-276-11-0-0-0` | 210 | 200210020 |
| `071101/6174383` | `7-14-55-1-0-0-0` | 207 | 200207009 |

## وب‌سرویس (پیش‌فرض — همان WinTestService موفق)

```
http://mdc-rayvarzsvc.itc.mashhad.ir/safa_shahrsazi_v2/WCFServer.ReceiveIncmVchrServices.svc
```

WSDL: همان آدرس + `?wsdl` (از شبکه داخلی / VPN ITC).

### MSB (Production — فقط وقتی IT مسیر را باز کرد)

```
http://msb.mashhad.ir/FavaFinancialServices/Rayvarz/VasetDaraamad/Proxy/WCFServer.ReceiveIncmVchrServices.svc
```

در `appsettings.json` مقدار `ServiceUrlMsb` نگه داشته شده؛ برای سوئیچ به MSB، `ServiceUrl` و `WsAddressingTo` را با همان آدرس MSB عوض کنید (ترجیحاً از سرور مجاز شهرسازی).

### نمونه appsettings

```json
"Rayvarz": {
  "ServiceUrl": "http://mdc-rayvarzsvc.itc.mashhad.ir/safa_shahrsazi_v2/WCFServer.ReceiveIncmVchrServices.svc",
  "WsAddressingTo": "http://mdc-rayvarzsvc.itc.mashhad.ir/safa_shahrsazi_v2/WCFServer.ReceiveIncmVchrServices.svc",
  "ServiceUrlMsb": "http://msb.mashhad.ir/FavaFinancialServices/Rayvarz/VasetDaraamad/Proxy/WCFServer.ReceiveIncmVchrServices.svc",
  "SoapAction": "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
  "PhasTyp": "ptDraftRegion",
  "VchrTyp": "pfRecieve",
  "IncmMkrTyp": "auto",
  "SoapEnvelopeStyle": "addressing",
  "SoapVersion": "soap12",
  "DryRun": true
}
```

### جریان تست

1. `DryRun: true` — فقط XML
2. فیش **غیرتکراری**
3. `DryRun: false` — ارسال واقعی (روی ITC سند ثبت می‌شود — سند تست را حذف کنید)
4. بررسی در `ray.incmdocsys` با سال شمسی: `WHERE yr=1405 AND RowDocNo=@FicheNo`

`PhasTyp` / `VchrTyp` در **XML SOAP** به‌صورت **نام عضو enum** ارسال می‌شوند (مثل WinTestService / DataContractSerializer)، نه عدد خام — پیش‌فرض: `ptDraftRegion` (حواله شهرستان، معادل PDF=7)، `pfRecieve` (دریافت، معادل PDF=0). در appsettings می‌توانید همان نام enum یا عدد PDF را بگذارید؛ برنامه عدد را به نام تبدیل می‌کند.

برای endpoint داخلی ITC (`mdc-rayvarzsvc`) حالت هم‌راستا با سامانه اصلی: `"SoapEnvelopeStyle": "addressing"` + `"SoapVersion": "soap12"`.
`IncmMkrTyp` را روی `"auto"` بگذارید: برای نوسازی/صنفی مقدار `1` و برای درآمد مقدار `0` تنظیم می‌شود.
`RefRowDocNoInDetail` فقط برای درآمد کاربرد دارد؛ برای نوسازی/صنفی خودکار `0` ارسال می‌شود (مطابق نمونه‌های موفق).


یک **GET ساده به آدرس WSDL** همان `ServiceUrl` (مثلاً `...?wsdl`) است — بدون ارسال `SaveDocument`. فقط می‌گوید از این ماشین به MSB/رایورز **راه شبکه و SSL** باز است یا نه.

- API: `GET /api/rayvarz-ping`
- UI: دکمه **«تست اتصال رایورز (Ping)»**

اگر Ping خطا دهد، مشکل SOAP/هدر نیست. اگر Ping موفق و Send خطا دهد، روی XML/هدر تمرکز کنید.

### لاگ و Diagnostics

- کنسول `dotnet run`: لاگ‌های `RayvarzClient` (سطح `Debug` در appsettings)
- پس از ارسال: فیلد `Diagnostics` در JSON (Category، Stage، HasWsAddressingHeader، ExceptionChain، LikelyCause)

اگر endpoint خطای 415 (Unsupported Media Type) داد: اول `SoapVersion=soap12` و `SoapEnvelopeStyle=addressing` را چک کنید.

آدرس MSB در `ServiceUrlMsb` برای سوئیچ Production نگه داشته شده است.

## عیب‌یابی

| مشکل | راه‌حل |
|------|--------|
اگر **Ping** با `502` روی `?wsdl` خطا داد ولی **تست POST (بدون ثبت)** موفق بود، طبیعی است — پروکسی MSB گاهی WSDL را 502 می‌دهد ولی `SaveDocument` با POST کار می‌کند. معیار ارسال: POST Test و سپس ارسال فیش.

| Ping OK ولی Send با `forcibly closed` | ۱) **تست POST (بدون ثبت)** ۲) **تست SaveDocument حداقلی** ۳) ارسال فیش. اگر 415 گرفتید → `SoapVersion=soap12` + `SoapEnvelopeStyle=addressing`. اگر (۱) و (۲) OK ولی (۳) reset/502 شد → محتوای فیش را با نمونهٔ موفق مقایسه کنید (IncmMkrTyp/Reason/Ref/Qty/RefRowDocNo). |
| Ping: `SSL connection could not be established` / `forcibly closed` | **شبکه/MSB** — WSDL بدون SOAP است؛ `SoapEnvelopeStyle` و XML بی‌اثرند. اجرا از سرور شهرسازی + VPN؛ `UseSystemProxy: true` یا `ProxyUrl`؛ تست مرورگر/curl به `ServiceUrl?wsdl` از همان PC |
| BnkAcntNo خالی | برای نوسازی: `OtherFields` — برای درآمد: join `Base_NosaziCode` |
| تکراری | فیش در `ray.incmdocsys` هست |
| فیش یافت نشد | `Income_Fiche` سپس `Duty_Fiche` |

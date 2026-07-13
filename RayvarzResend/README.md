# RayvarzResend — فرم تست ارسال مجدد به رایورز

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
  "DryRun": true
}
```

با `DryRun: true` فقط XML ساخته می‌شود و POST نمی‌زند.

### 3) اجرا

```powershell
cd RayvarzResend\RayvarzResend.Web
dotnet run
```

مرورگر: `http://localhost:5000` یا آدرسی که در کنسول نمایش داده می‌شود.

## استفاده

1. نوع شناسه: `FicheNo` یا `BillID+PaymentID`
2. یک فیش تست وارد کنید (مثلاً `971030914186`)
3. شعبه و تاریخ را انتخاب کنید
4. **دریافت فیش** — اطلاعات و ردیف‌های IncmNo نمایش داده می‌شود
5. **پیش‌نمایش XML** — SOAP را ببینید
6. **ارسال** — بعد از `DryRun: false`

## جریان

```
ورود دستی فیش
    → بارگذاری از Income_Fiche / Duty_Fiche
    → چک تکراری در ray.incmdocsys
    → نمایش BnkAcntNo + IncmNo rows
    → Preview XML
    → Reset وضعیت (اختیاری) + SaveDocument
    → بررسی Accounting_DocNotSent در صورت خطا
```

## ارسال واقعی

```json
"Rayvarz": {
  "ServiceUrl": "http://mdc-rayvarzsvc.itc.mashhad.ir/safa_shahrsazi_v2/WCFServer.ReceiveIncmVchrServices.svc",
  "DryRun": false
}
```

## فیش‌های تست پیشنهادی

| نوع | FicheNo |
|-----|---------|
| درآمد | 971030914186 |
| نوسازی | 091197/2947568 |

## عیب‌یابی

| مشکل | راه‌حل |
|------|--------|
| فیش یافت نشد | شناسه و connection string را چک کنید |
| تکراری | فیش در incmdocsys هست — ارسال نمی‌شود |
| RowGuid NULL | Id/SourceId از appsettings (`Rayvarz:SourceSystemId`) پر می‌شود — پیش‌فرض `11111` |
| خطای شبکه | VPN / URL تست |

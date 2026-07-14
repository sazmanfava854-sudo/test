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
| TransactionId | `NidFiche` (GUID) |
| SourceId | `appsettings → SourceSystemId` (`11111`) |
| RowDocNo | `FicheNo` |
| Ref2 / Ref3 | `BillID` / `PaymentID` |
| Qty | `Payable` (مبلغ کل) |
| Val | مبلغ هر ردیف |
| Bank | `ConfirmBankCode` |
| RowDate | `BankPaymentDate` → `PaymentDate` → `PrintDate` → `ExportDate` |
| branch / Fund | منطقه فیش (`OtherFields → منطقه` برای نوسازی) |

## فیش‌های تست تأییدشده

| FicheNo | BnkAcntNo | branch | Fund |
|---------|-----------|--------|------|
| `101104/9881711` | `10-8-276-11-0-0-0` | 210 | 200210020 |
| `071101/6174383` | `7-14-55-1-0-0-0` | 207 | 200207009 |

## عیب‌یابی

| مشکل | راه‌حل |
|------|--------|
| BnkAcntNo خالی | برای نوسازی: `OtherFields` — برای درآمد: join `Base_NosaziCode` |
| تکراری | فیش در `ray.incmdocsys` هست |
| فیش یافت نشد | `Income_Fiche` سپس `Duty_Fiche` |

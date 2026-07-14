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

با `DryRun: true` فقط XML ساخته می‌شود و POST نمی‌زند.

### 3) اجرا

```powershell
cd RayvarzResend\RayvarzResend.Web
dotnet run
```

مرورگر: `http://localhost:5000`

## مپینگ BnkAcntNo (مهم)

| نوع فیش | جدول | منبع BnkAcntNo | فرمت نمونه |
|---------|------|----------------|------------|
| **درآمد** | `Income_Fiche` | `Base_NosaziCode` از join با `Income → Sh_RequestInfo` | `10-8-276-11-1-0-0-0` |
| **نوسازی/صنفی** | `Duty_Fiche` | `OtherFields` → **کد نوسازی** (XML) | `7-14-55-1-0-0-0` |

> **توجه:** `BnkAcntNo` در رایورز «شماره حساب بانکی» نیست. برای نوسازی از کد XML فیش استفاده می‌شود، **نه** کد ثبت ملکی `Base_NosaziCode`.

### کوئری دستی BnkAcntNo

**نوسازی:**
```sql
SELECT OtherFields.value('(//ClsLog[Subject="کد نوسازي"]/Value)[1]', 'nvarchar(100)') AS BnkAcntNo
FROM dbo.Duty_Fiche WHERE FicheNo = @FicheNo;
```

**درآمد:**
```sql
SELECT CAST(b.CI_City AS varchar) + '-' + ... + CAST(b.Shop AS varchar) AS BnkAcntNo
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
| SourceId | `appsettings → SourceSystemId` (پیش‌فرض `11111`) |
| RowDocNo | `FicheNo` |
| Ref2 | `BillID` |
| Ref3 | `PaymentID` |
| Qty | `Payable` (مبلغ کل) |
| Val | مبلغ هر ردیف از `Income_Calculation` یا `Duty_FicheSub` |
| Bank | `ConfirmBankCode` (فقط اگر پرداخت شده) |
| RowDate | `BankPaymentDate` → `PaymentDate` → `PrintDate` → `ExportDate` |
| Fund / branch | انتخاب منطقه (برای نوسازی از `OtherFields → منطقه` پیشنهاد می‌شود) |

## جریان

```
ورود دستی فیش
    → بارگذاری از Income_Fiche / Duty_Fiche
    → چک تکراری در ray.incmdocsys
    → نمایش BnkAcntNo + منبع آن + IncmNo rows
    → Preview XML
    → Reset وضعیت + SaveDocument
```

## فیش‌های تست

| نوع | FicheNo | BnkAcntNo نمونه |
|-----|---------|-----------------|
| درآمد | 971030914186 | از Base_NosaziCode |
| نوسازی (در رایورز) | 071101/6174383 | `7-14-55-1-0-0-0` |
| نوسازی (ارسال نشده) | 071104/0073029 | `7-14-55-1-0-0-0` |

## عیب‌یابی

| مشکل | راه‌حل |
|------|--------|
| فیش یافت نشد | شناسه و connection string را چک کنید |
| تکراری | فیش در incmdocsys هست — ارسال نمی‌شود |
| BnkAcntNo اشتباه | نوع فیش را چک کنید — درآمد و نوسازی منبع متفاوت دارند |
| خطای شبکه | VPN / URL تست |

# تهاتر — ارسال از مسیر جدول واسط Sara

تهاتر با مسیر SOAP عادی (نوسازی/صنفی/درآمد شهرسازی) فرق دارد.

## تفاوت با نوسازی / صنفی

| | نوسازی / صنفی / درآمد عادی | تهاتر |
|--|---------------------------|--------|
| ورود | تک فیش یا (در فرایند Sara) اکسل | **فقط تک‌کد FicheNo** — بدون اکسل |
| ارسال | SOAP `SaveDocument` از RayvarzResend | تریگر وضعیت در `Income_Fiche`؛ رایورز از **جدول واسط** برمی‌دارد |
| چک تکراری | `ray.incmdocsys` | `dbo.Accounting_DocHeader` |

## جریان

1. `SELECT FicheNo FROM Accounting_DocHeader WHERE FicheNo = …`  
   اگر بود → **ارسال لازم نیست**.
2. `SELECT` از `Income_Fiche` و **نگه‌داشت**:  
   `ExportPermanentDate, PaymentBreakDate, PaymentDate, UserConfirmDate, UsernameUserConfirm, NidUserUserConfirm`
3. `UPDATE` وضعیت **۲** (معادل «تایید فیش دستی» در Sara):  
   `EumFicheStatus=2`, `ExportPermanentDate/PaymentBreakDate=امروز`, `PaymentDate=''`
4. انتظار پر شدن `Accounting_DocHeader` (poll)
5. `UPDATE` بازگردانی وضعیت **۳** با همان مقادیر SELECT اولیه
6. اگر هنوز در واسط نبود → `Accounting_DocNotSent.Comment` (مثلاً مرکز هزینه)

## API

```http
POST /api/tahator/check
{ "ficheNo": "040933318150" }

POST /api/tahator/send
{ "ficheNo": "040933318150" }
```

## تنظیمات

```json
"Tahator": {
  "PollIntervalMs": 2000,
  "PollTimeoutSeconds": 60
}
```

`DryRun`: اگر `Tahator:DryRun` نباشد از `Rayvarz:DryRun` ارث می‌برد. در DryRun هیچ UPDATE واقعی زده نمی‌شود.

## UI

بخش «تهاتر» در پایین صفحه اصلی — ورود تک فیش + بررسی + اجرای فرایند.

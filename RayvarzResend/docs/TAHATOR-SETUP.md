# تهاتر — SOAP + جدول واسط

تهاتر با مسیر عادی نوسازی/صنفی فرق دارد، ولی **ارسال به رایورز با SOAP** (`SaveDocument`) انجام می‌شود.

## تفاوت با نوسازی / صنفی

| | نوسازی / صنفی | تهاتر |
|--|---------------|--------|
| ورود | تک فیش | **فقط تک‌کد FicheNo** — بدون اکسل |
| DocTyp | ۱ / ۲ | **۱۴ یا ۱۵** (مثل `Tahator1` در Member 1388: Bank=4→۱۴ وگرنه ۱۵) |
| قبل از SOAP | ریست وضعیت Duty/Income | نگه‌داشت فیلدها + وضعیت **۲** |
| بعد از SOAP | تأیید `incmdocsys` | بازگردانی وضعیت **۳** + چک `Accounting_DocHeader` / `incmdocsys` / `DocNotSent` |

## جریان

1. اگر در `Accounting_DocHeader` یا `incmdocsys` بود → ارسال لازم نیست  
2. `SELECT` و نگه‌داشت از `Income_Fiche`  
3. `UPDATE` وضعیت **۲** (معادل تایید فیش دستی Sara)  
4. ساخت SOAP از **DSL / ActiveEngine** با `DocTyp` تهاتر و `POST SaveDocument`  
5. `UPDATE` بازگردانی وضعیت **۳** با مقادیر اولیه  
6. اگر در واسط / رایورز نبود → علت از `Accounting_DocNotSent`

## API

```http
POST /api/tahator/check
{ "ficheNo": "040933318150" }

POST /api/tahator/send
{
  "ficheNo": "040933318150",
  "branch": 207,
  "fund": 200207009,
  "docDate": "14050323",
  "actDate": "14050323",
  "dueDate": "14050323"
}
```

`branch` / تاریخ‌ها اختیاری‌اند؛ در صورت خالی از فیش / FundMap پر می‌شوند.

## تنظیمات

```json
"Tahator": {
  "PollIntervalMs": 2000,
  "PollTimeoutSeconds": 60
}
```

DryRun از `Rayvarz:DryRun` ارث می‌برد (مگر `Tahator:DryRun` ست شود). در DryRun: SOAP ساخته می‌شود ولی POST واقعی و UPDATE وضعیت زده نمی‌شود.

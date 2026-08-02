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
2. `SELECT` از `Income_Fiche`  
3. **ذخیره پایدار** همان مقادیر در `RayvarzRuleEngine.dbo.TahatorRestoreSnapshot` (Status=`Pending`)  
4. `UPDATE` وضعیت **۲** روی Sara  
5. ساخت SOAP از **DSL / ActiveEngine** با `DocTyp` تهاتر و `POST SaveDocument`  
6. `UPDATE` بازگردانی وضعیت **۳** از همان snapshot ذخیره‌شده → Status=`Restored`  
7. اگر در واسط / رایورز نبود → علت از `Accounting_DocNotSent`

اگر فرایند وسط کار قطع شود، snapshot با Status=`Pending` می‌ماند:

```http
GET  /api/tahator/pending
POST /api/tahator/restore
{ "ficheNo": "040933318150" }
```

اسکریپت جدول: `database/05_TahatorRestoreSnapshot.sql` (روی سرور ۲۳۲ / `RayvarzRuleEngine`). در صورت نبود جدول، سرویس در اولین استفاده آن را می‌سازد.

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

## Centers (مطابق Tahator1)

| فیلد SOAP | منبع |
|-----------|------|
| `DocumentItem.Center` | Bank=2 → `CreditorPapers` وگرنه `0` |
| `DocumentItemIncm.Center1` | `Income_Fiche.Deposit` |
| `DocumentItemIncm.Center2` | معمولاً خالی / `0` |
| `DocumentItemIncm.Center3` | `CheckNo=5` → `700100002` وگرنه `700100001` |
| `IncmNo` | Bank=4 → `200098` وگرنه `200099` |
| `Val` | `−Payable` |

## Golden تهاتر

اسکریپت: `database/06_RuleGolden_Seed_Tahator.sql` (بعد از schema و در صورت نیاز `04`).

| Id | FicheNo | Val | Center1 | Center3 |
|----|---------|-----|---------|---------|
| 11 | `050933483716` | −22,106,681,457 | 320008535 | 700100001 |
| 12 | `051133444502` | −5,676,696,274 | 320008535 | 700100001 |
| 13 | `051133450714` | −3,603,899,024 | 320008535 | 700100001 |
| 14 | `051233468141` | −26,841,652,707 | 320008535 | 700100001 |

```powershell
# روی RayvarzRuleEngine:
# database/06_RuleGolden_Seed_Tahator.sql
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/golden/dry-run" |
  Select-Object engineName, total, passed, allPassed
```

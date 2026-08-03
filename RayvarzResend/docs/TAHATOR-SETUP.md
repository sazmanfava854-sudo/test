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

## Centers — منبع: تابع `Tahator1` در XmlBody (Member 1388)

این منطق از **۴ فیش نمونه استنتاج نشده**؛ از بدنهٔ VB تابع `Tahator1` («تهاتر تک مبلغی») خوانده شده است.
شرط ورود همان تابع: `CI_IncomeAccountGroup = 157`.

نقل قول مستقیم از XmlBody:

```vb
' Center
If CI_Bank = "2" Then
    Refcenter.Value = CreditorPapers.ToString()
Else
    Refcenter.Value = "0"
End If

' Center1
Refcenter1.Value = deposit

' Center3  — در Tahator1 هیچ Center2 ست نمی‌شود
If CheckNo = "5" Then
    Refcenter3.Value = "700100002"
Else
    Refcenter3.Value = "700100001"
End If
```

| فیلد SOAP | منبع در `Tahator1` |
|-----------|---------------------|
| `DocumentItem.Center` | `CI_Bank="2"` → `CreditorPapers` وگرنه `"0"` |
| `DocumentItemIncm.Center1` | `IncomeFiche.deposit` |
| `DocumentItemIncm.Center2` | در `Tahator1` ست نمی‌شود |
| `DocumentItemIncm.Center3` | `CheckNo="5"` → `700100002` وگرنه `700100001` |
| `IncmNo` / `WrapperAccountNo` | `CI_Bank=4` → `200098` وگرنه `200099` |
| `Val` / `Price` | `(-1) * Payable` |

**تفاوت با تابع `Tahator` («درآمدی تهاتر»):** آنجا `Center1` ثابت `335000181` است (یا در شاخهٔ دیگر Centers منطقه‌ای مثل `910700001` / `335000046` / `800800007`). مسیر فعلی resend/گلدن فقط **`Tahator1` + گروه ۱۵۷** را پیاده می‌کند؛ نمونه‌های گلدن هم همه `CI_IncomeAccountGroup=157` هستند.

۴ فیش گلدن فقط برای **مقادیر expected** (Val/Center1/…) استفاده شدند تا dry-run با `incmdocsys` چک شود، نه برای کشف قانون.

## Golden تهاتر

اسکریپت: `database/06_RuleGolden_Seed_Tahator.sql` (بعد از schema و در صورت نیاز `04`).

| Id | FicheNo | Val | Center1 | Center3 |
|----|---------|-----|---------|---------|
| 11 | `050933483716` | −22,106,681,457 | 320008535 | 700100001 |
| 12 | `051133444502` | −5,676,696,274 | 320008535 | 700100001 |
| 13 | `051133450714` | −3,603,899,024 | 320008535 | 700100001 |
| 14 | `051233468141` | −26,841,652,707 | 320008535 | 700100001 |

## اختلاف تستی در برابر سند اصلی (نمونه `040933318150`)

| فیلد | تست (اشتباه قبلی) | اصلی / Tahator1 | وضعیت |
|------|-------------------|-----------------|--------|
| DocTyp / IncmNo / Val / Center1 / Center3 | ✔ | ✔ | درست بود |
| `RowDocNo` | `040933/318150` | `040933318150` | اسلش حذف می‌شود |
| `Branch` | 201 (منطقه UI) | **102** (ثابت اسناد تهاتر) | اصلاح شد |
| `Fund` | 200201012 (FundMap نوسازی) | **59** (Tahator1: منطقه۹→209→59) | اصلاح شد |
| `BnkAcntNo` | `2-9-3-…` (City اول) | `9-3-161-2-1-0-0` (Nick) | اصلاح شد |
| `PhasTyp` | 7 | **2** | اصلاح شد |
| `VchrTyp` | 0 | **1** | اصلاح شد |
| `ActTyp` | 3 | **1** | اصلاح شد |
| `Bank` | 4 (CI_Bank) | NULL/0 (PaymentBranch خالی) | اصلاح شد |
| تاریخ | روز ارسال | تاریخ پرداخت فیش | از فیش بیاید؛ در UI خالی بگذارید |

بعد از pull، دوباره با `FicheNo=040933318150` (بدون اسلش) و DryRun تست کنید؛ در XML باید `Branch=102`، `Fund=59`، `PhasTyp=ptDraft`، `VchrTyp=pfPay` ببینید.


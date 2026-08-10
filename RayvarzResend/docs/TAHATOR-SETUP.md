# تهاتر — SOAP + جدول واسط

تهاتر با مسیر عادی نوسازی/صنفی فرق دارد، ولی **ارسال به رایورز با SOAP** (`SaveDocument`) انجام می‌شود.

## تفاوت با نوسازی / صنفی

| | نوسازی / صنفی | تهاتر مبلغ (۱۵۷) | تهاتر درآمدی (۱۵۸) |
|--|---------------|------------------|---------------------|
| ورود | تک فیش | **فقط FicheNo** | **فقط FicheNo** |
| تابع Member | — | `Tahator1` | `Tahator` |
| مقصد | منطقه/صنفی | **مرکز** Branch=**۱۰۲** | **منطقه** Branch=**۲۰۱–۲۱۲** |
| DocTyp | ۱ / ۲ | Bank=4→**۱۴** وگرنه **۱۵** | Bank=4→**۱۷** وگرنه **۱۸** |
| Fund | FundMap نوسازی | **۵۱–۶۳** | **۳۱–۴۲** |
| Val | مثبت | **منفی** (−Payable) | **مثبت** (Income_Calculation) |
| قبل از SOAP | ریست | نگه‌داشت + وضعیت **۲** | همان |
| بعد از SOAP | `incmdocsys` | وضعیت **۳** + واسط | همان |

## جریان

1. **جفت تهاتر** — هر عملیات تهاتر دو فیش `Income_Fiche` دارد (همان `NidIncome`):
   - **۱۵۷** مبلغ / `Tahator1` / Branch **۱۰۲** / DocTyp **۱۴|۱۵**
   - **۱۵۸** درآمد / `Tahator` / Branch **۲۰۱–۲۱۲** / DocTyp **۱۷|۱۸**
   - ارسال: **اول ۱۵۷، بعد ۱۵۸** (مثل Member 1388)
2. اگر **هر دو** در `incmdocsys` بود → ارسال لازم نیست (`Accounting_DocHeader` به‌تنهایی مانع ارسال مجدد نیست — سناریوی حذف از رایورز)  
3. `SELECT` از `Income_Fiche` (هر دو فیش)  
4. **ذخیره پایدار** snapshot هر فишی که ارسال می‌شود  
5. `UPDATE` وضعیت **۲** روی Sara (فقط فیش‌های در صف ارسال)  
6. ساخت SOAP و `POST SaveDocument` — **دو بار** در صورت نیاز  
7. `UPDATE` بازگردانی وضعیت **۳**  
8. اگر در واسط / رایورز نبود → علت از `Accounting_DocNotSent`

اگر فرایند وسط کار قطع شود، snapshot با Status=`Pending` می‌ماند:

```http
GET  /api/tahator/pending
POST /api/tahator/restore
{ "ficheNo": "040933318150" }
```

اسکریپت جدول: `database/05_TahatorRestoreSnapshot.sql` (روی سرور ۲۳۲ / `RayvarzRuleEngine`). در صورت نبود جدول، سرویس در اولین استفاده آن را می‌سازد.

## مراحل ۲ و ۳ — نگه‌داشت + وضعیت ۲ (تاریخ روز)

| مرحله | کار | نتیجه روی `Income_Fiche` |
|-------|-----|---------------------------|
| ۲ | `SELECT` فیلدهای اصلی + ذخیره در `TahatorRestoreSnapshot` | تغییری روی Sara نیست |
| ۳ | `UPDATE` وضعیت **۲** | `ExportPermanentDate`/`PaymentBreakDate` = **تاریخ روز**، **`PaymentDate=''` (عمدی خالی)**؛ UserConfirm* دست نخورده |
| بعد از SOAP موفق | وضعیت **۳** | **همه فیلدها از snapshot اصلی** — Export/Break/PaymentDate/UserConfirm |
| بعد از SOAP ناموفق | بازگردانی کامل | همان — همه فیلدها از snapshot |

`PaymentDate` قبل از تریگر ممکن است مقدار داشته باشد (مثل `1404/12/11`). در وضعیت ۲ **عمداً خالی** می‌شود؛ بعد از ارسال (موفق یا ناموفق) **همه تاریخ‌ها از snapshot برمی‌گردند**.

تاریخ SOAP تهاتر (`DocDate`/`ActDate`/`Due`) هم پیش‌فرض **امروز** است — نه `PaymentDate` فیش.

اگر بعد از ارسال کامل دوباره SELECT بزنید، تاریخ‌ها **اصلی** هستند (بازگردانی شده) — این درست است.

چرا «تاریخ روز» نمی‌بینید؟

1. **`Rayvarz:DryRun=true`** در پروسه جاری → UPDATE روی Sara زده نمی‌شود  
   بعد از تغییر به `false` **حتماً Restart** کنید؛ فقط ذخیره فایل کافی نیست.
2. فیش در **incmdocsys** هست → Skip (`InRayvarz`) — حتی اگر در واسط نباشد  
   فقط در **Accounting_DocHeader** بود ولی از رایورز حذف شده → **ارسال مجدد** انجام می‌شود
3. بعد از SOAP موفق هم Export/Break به مقادیر اصلی snapshot برمی‌گردد (مثل UPDATE دستی وضعیت ۳)

در پاسخ API به خط `0) ... DryRun=...` و `3a) تاریخ SOAP` نگاه کنید.

### تست فقط مرحله ۲ و ۳ (دیدن تاریخ روز)

```json
// appsettings: "Rayvarz": { "DryRun": false }
POST /api/tahator/send
{
  "ficheNo": "040933/318150",
  "force": true,
  "holdAfterStatus2": true
}
```

از UI: چک‌باکس‌های «force» و «توقف روی وضعیت ۲» را بزنید (DryRun باید false باشد).

سپس در Sara:
```sql
SELECT ExportPermanentDate, PaymentBreakDate, PaymentDate,
       UserConfirmDate, UsernameUserConfirm, NidUserUserConfirm, EumFicheStatus
FROM dbo.Income_Fiche WHERE FicheNo = '040933/318150';
-- انتظار: Status=2 ، Export/Break = امروز ، PaymentDate خالی (عمدی) ، UserConfirm* همان اصلی
-- PaymentDate اصلی (مثلاً 1404/12/11) فقط در snapshot است تا restore
-- توجه: '040933/318150' ≠ '040933318150'
```

بازگردانی:
```http
POST /api/tahator/restore
{ "ficheNo": "040933/318150" }
```
یا دکمه «بازگردانی وضعیت ۳» در UI.


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

## مسیر دوم — تابع `Tahator` / گروه ۱۵۸ («درآمدی تهاتر») → منطقه

منبع: XmlBody کامل Member 1388 (`Fixtures/member-1388-full-body.vb`).

شرط ورود: `CI_IncomeAccountGroup = 158`.

| فیلد SOAP | مقدار از VB `Tahator` |
|-----------|------------------------|
| `Mess.District` / branch | **DistrickBranch** منطقه (۲۰۱–۲۱۲) |
| `Fund` | ۲۰۱→۳۱ … ۲۱۲→۴۲ ؛ ۲۱۸→۴۳ |
| `DocTyp` | `CI_Bank=4` → **۱۷** وگرنه **۱۸** |
| `PhasType` | **۷** (`ptDraftRegion`) |
| `vchrtyp` | **۰** (`pfRecieve`) |
| `ActTyp` | **۱** |
| `docdsc` | `اسناد تهاتر درامد` |
| `DocTypDsc` | `عوارض تهاتر درامد` |
| `Center` | Bank=2 → CreditorPapers وگرنه ۰ |
| `Center1` | ثابت **`335000181`** |
| `Ref` | Bank=4 → ۴ وگرنه ۲ |
| `FileNo` | `DepositID` |
| `Val` | مثبت؛ ردیف‌های `Income_Calculation` اسکیل به Payable |

مقصد (مرکز/منطقه) از **گروه حساب**؛ DocTyp از **CI_Bank**.

۴ فیش گلدن فعلی فقط مسیر **۱۵۷ / مبلغ** را پوشش می‌دهند.

### Fixture DSL

- `RuleEngine/Parser/Fixtures/member-1388-full.xml` — ClsFunction با Body کامل (شامل `Tahator` / `Tahator1`)
- `member-1388-full-body.vb` — همان VB خام
- برای LocalXmlPath: مسیر `member-1388-full.xml` را در `RuleEngine:LocalXmlPath` بگذارید تا DSL از همین XmlBody ساخته شود
- SOAP فیلدهای تهاتر از پورت C# همان VB (`TahatorRowBuilder`) پر می‌شود

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


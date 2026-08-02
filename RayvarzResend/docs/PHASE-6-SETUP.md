# فاز ۶ — گسترش Parser + توابع درآمدی (iNcOME*)

پیش‌نیاز: فاز ۴ (Promote + Dynamic فعال برای Nosazi).

## هدف

| # | کار | وضعیت |
|---|-----|--------|
| 6.1 | Parser: `iNcOME`, `iNcOMEOragh`, `iNcOMESeprdeh`, `iNcOMEEshghal`, … | ✅ |
| 6.2 | Operation: `Income.BuildIncomeRows` | ✅ |
| 6.3 | قوانین قبل از SOAP (کلی / وابسته به نوع فیش)؛ ردیف از فیش live | ✅ |
| 6.4 | Golden Income (اختیاری) | ⏳ در صورت نیاز seed جدا |

## الگوی اجرا (مثل Nosazi)

```
Run()  ← قوانین قبل از SOAP
  If ObjOnPrice=Income → Call iNcOME* / BazAfarine / Tahator…
  Else → Call Nosazi()
        │
        ▼
  Build*Rows  ← ردیف‌ها از Sara + اسکیل به PayablePrice
  Validate.RowSumEqualsPayable + نقش‌های اجباری
        │
        ▼
  SOAP
```

هیچ تابعی `Unsupported (بدنه اجرا نمی‌شود)` نیست. توابع **کلی** (مثل `ChangeDate`, `FnSMS`) برای همه نوع فیش؛ توابع **وابسته** (`Nosazi` / `iNcOME*` / `Tahator`) فقط وقتی نوع فیش مرتبط است اعمال می‌شوند. خطوط VB خارج از subset به‌صورت خط‌به‌خط defer می‌شوند؛ کل تابع skip نمی‌شود.

**تخفیف درآمد:** جمع خام `IncomeValue` اغلب ≠ `PayablePrice`. همان منطق `SoapBuilder.NormalizeRows` در `IncomeRowScaler.ScaleToPayable` هنگام Load و در `Income.BuildIncomeRows` اعمال می‌شود تا موتور/golden با مبلغ ارسالی به Rayvarz یکی باشد.

## توابع پشتیبانی‌شده

| تابع | نقش |
|------|-----|
| `Run` | EntryPoint |
| `ChangeDate`, `FnSMS`, … | Global (همه نوع فیش) |
| `Nosazi` | Duty |
| `iNcOME*` / `BazAfarine` / … | Income |
| `Tahator` / `Tahator1` | Tahator (DocTyp 14/15 قبل از SOAP اجباری) |
| هر `iNcOME*` | با پیشوند `iNcOME` هم پشتیبانی می‌شود |

`ParserVersion` → **2.3.0**

از 2.2.0 همه توابع Member (Public/Private) با `IsSupported=true` در DSL هستند.
از 2.3.0 بدنه توابع به‌خاطر Unsupported یکجا skip نمی‌شود؛ قوانین قبل از SOAP بر اساس نقش/نوع فیش اعمال می‌شوند.

## تست بعد از deploy

```powershell
# 1) rebuild snapshot با parser جدید
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/dsl/parse?force=true"
# انتظار: iNcOME در unsupportedFunctions نباشد (یا کمتر)

# 2) preview یک فیش درآمدی از UI / API

# 3) Duty طلایی همچنان سبز بماند
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/golden/dry-run" |
  Select-Object engineName, passed, allPassed
```

## Golden درآمد (نمونه‌های کاربر — فاز ۶)

فایل SQL:

```text
database/04_RuleGolden_Seed_Phase6_Samples.sql
```

روی `RayvarzRuleEngine` (سرور ۲۳۲) بعد از `02_RuleGolden_Seed.sql` اجرا کنید.

| Id | FicheNo | نوع | ردیف | Payable |
|----|---------|-----|------|---------|
| 5 | `050733453546` | Income شهرسازی | 5 | 5,379,066,000 |
| 6 | `050733451977` | Income شهرسازی | 3 | 2,024,365,000 |
| 7 | `050733447710` | Income شهرسازی | 5 | 1,780,716,000 |
| 8 | `050733454216` | Income شهرسازی | 4 | 147,291,000 |
| 9 | `071105/0385826` | Duty نوسازی | 4 | 38,688,000 |
| 10 | `071205/20381801` | Duty صنفی | 3 | 8,089,000 |

تهاتر (اسکریپت جدا: `database/06_RuleGolden_Seed_Tahator.sql`) — چک `Center` / `Center1` / `Center2` / `Center3`:

| Id | FicheNo | نوع | ردیف | Val (تهاتر) |
|----|---------|-----|------|-------------|
| 11–14 | `050933483716` … `051233468141` | Tahator DocTyp14 | 1 | −Payable ؛ Center1=Deposit ؛ Center3=700100001 |

منبع expected: `Ray_CityHall.ray.incmdocsys` — فیش‌ها در Sara (`Income_Fiche` / `Duty_Fiche`) موجودند.

```powershell
# بعد از اجرای SQL:
Invoke-RestMethod -Uri "http://localhost:5000/api/rule/golden"
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/golden/dry-run" |
  Select-Object engineName, total, passed, allPassed
# انتظار با Dynamic: total>=10 ، و income+duty سبز اگر ردیف‌های Sara با Rayvarz هم‌خوان باشند
```

`GoldenDryRunService` حالا `FicheCategory.Income` را می‌پذیرد.

## PayloadSource در برابر ActiveEngine

| تنظیم | معنی |
|--------|------|
| `Rayvarz:PayloadSource=LegacyCSharp` | SOAP داخل همین اپ ساخته می‌شود (نه SaraBridge) |
| `RuleSyncState.ActiveEngine=Dynamic` | همان ساخت از **DSL snapshot** (Run→Nosazi/iNcOME + Build*Rows) |
| `Rayvarz:PayloadSource=RuleEngineBridge` | فراخوانی Sara خارجی — DSL این پروژه نیست |

با `ActiveEngine=Dynamic`، preview/send از DSL می‌خواند. `payloadMode` در پاسخ ممکن است هنوز `LegacyCSharp` باشد (= مسیر in-process)؛ فیلد مهم `engineName` است.

```powershell
# تأیید: engineName باید Dynamic باشد
Invoke-RestMethod -Uri "http://localhost:5000/api/rule/engine" |
  Select-Object activeEngine, resolvedEngine, activeSnapshotId, payloadSource, dryRun
```

## ایمنی

- `DryRun=true` یعنی SOAP به Rayvarz پست نشود؛ موتور همچنان Dynamic/DSL است
- بعد از `dsl/parse?force=true` اگر candidate قبلاً Promoted است، promote دوباره لازم نیست (snapshot همان Id به‌روز می‌شود)
- Rollback: `POST /api/rule/promote/rollback`

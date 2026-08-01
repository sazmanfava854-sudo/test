# فازبندی پروژه Rule Engine — RayvarzResend

این سند **نقطه شروع** پیاده‌سازی است: ابتدا بازگشت به v16، سپس فازها به‌صورت دسته‌ای.

---

## ۰. نقطه بازگشت (قبل از هر فاز)

### نسخه امن v16 — C# ثابت / بدون Rule Engine DB

| نوع | مقدار |
|-----|--------|
| Tag | `rayvarz-resend-baseline-v16` |
| Commit | `766da11` |
| شاخه آرشیو | `baseline/rayvarz-resend-v16` (شامل `BASELINE-v16.md`) |
| Zip | `RayvarzResend-16.zip` |

```bash
git fetch origin
git checkout rayvarz-resend-baseline-v16
# یا
git checkout baseline/rayvarz-resend-v16
```

**محتوا:** `DutyNosaziLogic` / `FicheRepository` / SOAP — **بدون** XmlBody→DSL→Executor.

### شاخه توسعه Rule Engine

| شاخه | نقش |
|------|-----|
| `rayvarz-resend` | توسعه اصلی |
| `cursor/*-59b2` | کار Cloud Agent |

**قانون:** هر فاز روی شاخه feature؛ v16 و `baseline/*` **هرگز rewrite نشود**.

### حالت اجرای تولید تا پایان فاز ۴

```json
"Rayvarz": { "PayloadSource": "LegacyCSharp" }
```

Dynamic فقط بعد از Promote + Dry Run goldenها فعال می‌شود.

---

## نمای کلی فازها (دسته‌بندی)

```
┌─────────────────────────────────────────────────────────────────┐
│  گروه A — زیرساخت و داده (بدون Parser VB)          فاز ۰، ۱    │
├─────────────────────────────────────────────────────────────────┤
│  گروه B — موتور قوانین (XmlBody → DSL → Execute)   فاز ۲، ۳، ۴ │
├─────────────────────────────────────────────────────────────────┤
│  گروه C — کیفیت و تولید                            فاز ۵، ۶    │
└─────────────────────────────────────────────────────────────────┘
```

| گروه | فاز | عنوان | خروجی قابل تحویل |
|------|-----|--------|------------------|
| **A** | **۰** | زیرساخت DB + Golden + Sync اسکلت | `RayvarzRuleEngine` DB، seed ۴ فیش، خواندن History/Member |
| **A** | **۱** | Sara live + Legacy پایدار | همان resend v16؛ فیش همیشه از Sara |
| **B** | **۲** | Parser ClsFunction (subset) | `Run` → `Nosazi` به AST؛ بدون اجرا |
| **B** | **۳** | Operation Registry + Executor | اجرای AST با رفتار تقلیدی Sara |
| **B** | **۴** | Version Manager + Promote | 72h، hash، Dry Run، Active Dynamic |
| **C** | **۵** | هم‌ترازی Golden با Rayvarz | Assert سطح B؛ Legacy vs Dynamic |
| **C** | **۶** | گسترش Parser + Income | `iNcOME` و سایر `Call`های `Run` |

---

## فاز ۰ — زیرساخت (گروه A)

**هدف:** آماده‌سازی بدون تغییر رفتار تولید.

| # | کار | جزئیات |
|---|-----|--------|
| 0.1 | DB `RayvarzRuleEngine` | جداول: `RuleSyncState`, `RuleCandidate`, `RuleDslSnapshot`, `RulePromotionLog`, `RuleDryRunResult`, `RuleGoldenFiche`, `RuleGoldenExpectedRow` |
| 0.2 | Seed Golden | ۴ فیش: `101104/9881711`, `051204/19920388`, `021204/19379176`, `111104/9485929` + expected از `ray.incmdocsys` |
| 0.3 | Connection سرور ۲۳۲ | `Sara8M03` (فیش live)، `DbRuleEngein` (read-only Member/History) |
| 0.4 | `MemberRuleRepository` | `Member` + `MemberHistory`؛ کلید `NidHistory` + `ModifyDate`+`ModifyTime` |
| 0.5 | `RuleHistoryChecker` | مقایسه با `RuleSyncState.LastSeenNidHistory` |
| 0.6 | اسکلت سرویس‌ها | بدون Parser کامل؛ `PayloadSource` = `LegacyCSharp` |

**تعریف done:** migration اجرا شود؛ API/health بتواند آخرین History و golden meta را بخواند؛ **resend همان v16**.

---

## فاز ۱ — Sara live یکپارچه (گروه A)

**هدف:** یک مسیر داده برای تولید و تست.

| # | کار |
|---|-----|
| 1.1 | `FicheRepository` تنها منبع فیش (Duty_Fiche + Sub) |
| 1.2 | `GoldenDryRunService` — بارگذاری live + assert ساختاری (بدون DSL) در برابر Legacy |
| 1.3 | `IFicheRuleEngine` + `LegacyRuleEngine` = `DutyNosaziLogic` + `SoapBuilder` |
| 1.4 | مستندات connection `appsettings` سرور ۲۳۲ |

**تعریف done:** ۴ golden با Legacy موفق؛ هیچ تغییری در SOAP خروجی نسبت به v16.

---

## فاز ۲ — Parser (گروه B)

**هدف:** `XmlBody` (ClsFunction) → `DslProgram` (AST).

| # | کار |
|---|-----|
| 2.1 | `XmlEnvelopeReader` — استخراج `Body` از `Member`/`History` |
| 2.2 | `VbTranspiler` subset — فقط `Run()` dispatch به `Nosazi()` |
| 2.3 | `DslModel` — Assign, If, CallOperation, CallFunction, TryCatch |
| 2.4 | Canonical XML hash — جلوگیری از rebuild بی‌دلیل |
| 2.5 | ذخیره snapshot در `RuleDslSnapshot` (هنوز Active نشود) |

**دامنه Parser فاز ۲:** `NidMember=1388`، توابع `Run` + `Nosazi`؛ بقیه → `Unsupported`.

**تعریف done:** parse نمونه XmlBody واقعی بدون خطا؛ AST در DB ذخیره شود؛ تولید همچنان Legacy.

---

## فاز ۳ — Registry + Executor (گروه B)

**هدف:** اجرای AST با Operationهای C# (رفتار مطابق Sara).

| # | کار |
|---|-----|
| 3.1 | `IOperationRegistry` — ~۲۶ operation فاز ۱ (محاسبه، doc row، district، ref) |
| 3.2 | `DslValidator` — operationهای ناشناخته = fail |
| 3.3 | `DslExecutor` — اجرای `Run` روی context فیش live |
| 3.4 | `DynamicRuleEngine` — بدون `TmpDocument.Save` در Dry Run |
| 3.5 | `RuleEngineFactory` — per-request fallback به Legacy |

**تعریف done:** Dry Run سطح A روی ۴ golden (ساختار و جمع مبالغ)؛ تولید هنوز Legacy مگر flag دستی dev.

---

## فاز ۴ — Version Manager + Promote (گروه B)

**هدف:** فعال‌سازی خودکار نسخه پایدار.

| شرط Promote | |
|-------------|--|
| XML معتبر | |
| Parse success | |
| Validation success | |
| Dry Run success (هر ۴ golden) | |
| ≥ 72 ساعت بدون رکورد جدیدتر در `MemberHistory` | |
| (توصیه) hash candidate = hash `Member.XmlBody` | |

| # | کار |
|---|-----|
| 4.1 | `RuleVersionManager` + background poll (۱۵ دقیقه) |
| 4.2 | State machine Candidate → Stable → Active |
| 4.3 | `RulePromotionLog` |
| 4.4 | `ActiveEngine` = Dynamic فقط بعد از promote |
| 4.5 | Circuit breaker → Legacy در خطای متوالی |

**تعریف done:** یک تغییر آزمایشی در History → بعد از 72h + تست‌ها → Active؛ rollback با `ActiveEngine=Legacy`.

---

## فاز ۵ — هم‌ترازی Rayvarz (گروه C)

**هدف:** Dynamic ≡ Legacy ≡ رکوردهای `ray.incmdocsys`.

| # | کار |
|---|-----|
| 5.1 | Assert سطح B — `IncmNo`, `Val`, `Branch`, `Bank`, `IncmRowDsc` |
| 5.2 | گزارش diff در `RuleDryRunResult` |
| 5.3 | سیاست: اختلاف > 0 → Promote ممنوع |

**تعریف done:** ۴ golden سطح B سبز با Dynamic قبل از promote واقعی در prod.

---

## فاز ۶ — گسترش (گروه C)

**هدف:** پوشش کامل `Run()`.

| # | کار |
|---|-----|
| 6.1 | Parser: `iNcOME`, `iNcOMESeprdeh`, … |
| 6.2 | Operationهای Income |
| 6.3 | goldenهای اضافه در صورت نیاز |
| 6.4 | (اختیاری) Sara DLL فقط validation |

---

## منابع داده (ثابت در همه فازها)

| داده | منبع | سرور |
|------|------|------|
| فیش (ورودی) | `Sara8M03.dbo.Duty_Fiche` + Sub | ۲۳۲ — **همیشه live** |
| قانون | `DbRuleEngein.dbo.Member` | ۲۳۲ — read |
| لاگ تغییر قانون | `DbRuleEngein.dbo.MemberHistory` | ۲۳۲ — read |
| انتظار تست | `RuleGoldenExpectedRow` | `RayvarzRuleEngine` |
| state / DSL | `RayvarzRuleEngine` | محلی یا ۲۳۲ |

### MemberHistory — ستون‌های کلیدی

`NidHistory`, `NidMember`, `XmlBody`, `ModifyDate`, `ModifyTime`, `Modifyer`, `ModifyDesc`

### Member — XmlBody فعال

`NidMember=1388`, `NidClass=360`, `isActive`, `Version`, `XmlBody`

---

## وابستگی فازها

```
فاز ۰ ──► فاز ۱ ──► فاز ۲ ──► فاز ۳ ──► فاز ۴ ──► فاز ۵
              │                                    │
              └──────── Legacy همیشه در دسترس ◄────┘
فاز ۶ (بعد از ۵)
```

**هیچ فازی نباید v16 را بشکند** — `LegacyCSharp` تا پایان فاز ۴ مسیر پیش‌فرض است.

---

## چک‌لیست شروع فاز ۰

- [ ] تأیید: بازگشت به `rayvarz-resend-baseline-v16` تست شد
- [ ] Connection string سرور ۲۳۲ در `appsettings.Development.json` (خارج git)
- [ ] تأیید ۴ FicheNo در Sara موجودند
- [ ] ایجاد DB `RayvarzRuleEngine`
- [ ] شاخه feature: `cursor/rule-engine-phase0-59b2`

---

## مراجع

- `BASELINE-v16.md` — بازگشت فوری
- `RULE-ENGINE-FEASIBILITY.md` — امکان‌سنجی Member/XmlBody
- `RULE-ENGINE-ARCHITECTURE.md` — PayloadSource و Bridge

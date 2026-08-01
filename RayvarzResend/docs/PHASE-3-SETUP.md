# فاز ۳ — Operation Registry + Executor + DynamicRuleEngine

پیش‌نیاز: فاز ۲ (Parser + RuleDslSnapshot).

## هدف

| # | کار | وضعیت |
|---|-----|--------|
| 3.1 | `IOperationRegistry` (~۲۶ operation) | ✅ |
| 3.2 | `DslValidator` — operation ناشناخته = fail | ✅ |
| 3.3 | `DslExecutor` — اجرای `Run` روی فیش live | ✅ |
| 3.4 | `DynamicRuleEngine` — بدون Save در DryRun | ✅ |
| 3.5 | `RuleEngineFactory` — fallback به Legacy | ✅ |

**تعریف done:** Golden dry-run سطح A با `ForceEngine=Dynamic`؛ تولید همچنان Legacy.

## معماری

```
RuleDslSnapshot (JSON AST)
        │
        ▼
  DslValidator ──► DslExecutor ──► OperationRegistry
        │                │              │
        │                │              └── DutyNosaziLogic, District, Date, …
        │                └── Run → Nosazi → BuildDutyRows
        ▼
 DynamicRuleEngine ──► SoapBuilder (اگر buildSoap)
        │ (خطا)
        └──► LegacyRuleEngine (fallback)
```

## Operationهای ثبت‌شده (نمونه)

| Key | نقش |
|-----|-----|
| `Info8.GetAccountingDocCreateParameter` | ساخت param (بدون DLL) |
| `ClsAccounting.Save` / `TmpDocument.Save` | **skip در DryRun** |
| `Nosazi.BuildDutyRows` | ردیف‌ها از فیش live |
| `Duty.CalculateSubAmounts` | همان nosazo.vb |
| `Duty.BuildIncmRows` | IncmNo/Val |
| `District.ResolveBranch/Fund` | شعبه/صندوق |
| `Validate.RowSumEqualsPayable` | کنترل جمع |

## تست Dynamic (dev)

در `appsettings.Development.json`:

```json
{
  "RuleEngine": {
    "ForceEngine": "Dynamic",
    "DynamicFallbackToLegacy": true
  }
}
```

```bash
# 1) parse + snapshot
curl -X POST http://localhost:5123/api/rule/dsl/parse

# 2) golden با Dynamic
curl -X POST http://localhost:5123/api/rule/golden/dry-run
# انتظار: engineName=Dynamic, passed=4, allPassed=true
```

## API جدید

| Endpoint | کار |
|----------|-----|
| `POST /api/rule/dsl/validate` | validate AST بدون اجرا |
| `POST /api/rule/golden/dry-run` | شامل `forceEngine` در پاسخ |

## تولید

```json
"Rayvarz": { "PayloadSource": "LegacyCSharp" }
"RuleEngine": { "ForceEngine": "" }
```

تا فاز ۴ Promote، `ActiveEngine` در DB = `Legacy`.

## فاز بعدی

فاز ۴: Version Manager Promote + `ActiveEngine=Dynamic` بعد از 72h + golden سبز.

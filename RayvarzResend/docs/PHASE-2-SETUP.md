# فاز ۲ — Parser XmlBody → DSL (AST)

پیش‌نیاز: فاز ۰ (DB) + فاز ۱ (`IFicheRuleEngine`).

## هدف

| # | کار | وضعیت |
|---|-----|--------|
| 2.1 | `XmlEnvelopeReader` — Body از Member/History + hash | ✅ |
| 2.2 | `VbTranspiler` — Run dispatch → Nosazi | ✅ |
| 2.3 | `DslModel` — Assign, If, CallOp, CallFn, TryCatch | ✅ |
| 2.4 | Canonical XML hash (skip rebuild) | ✅ |
| 2.5 | ذخیره در `RuleDslSnapshot` (IsActive=0) | ✅ |

**تولید:** همچنان `PayloadSource=LegacyCSharp` — Parser فقط parse/store، بدون اجرا.

## API

| Endpoint | کار |
|----------|-----|
| `POST /api/rule/dsl/parse` | بارگذاری Member فعال → parse → ذخیره snapshot |
| `POST /api/rule/dsl/preview` | parse بدون DB — body: `{ "xmlBody": "..." }` |
| `GET /api/rule/dsl/latest` | آخرین snapshot (meta) |

## تست سریع

### ۱) با Member واقعی (سرور ۲۳۲)

```bash
curl -X POST http://localhost:5123/api/rule/dsl/parse
curl http://localhost:5123/api/rule/dsl/latest
```

### ۲) با fixture نمونه (بدون DB)

```bash
curl -X POST http://localhost:5123/api/rule/dsl/preview \
  -H "Content-Type: application/json" \
  -d "{\"xmlBody\": \"$(cat RayvarzResend.Web/RuleEngine/Parser/Fixtures/member-1388-sample.xml | sed 's/\"/\\\\\"/g' | tr -d '\n')\"}"
```

یا در PowerShell فایل fixture را بخوانید و در `xmlBody` بگذارید.

پاسخ مورد انتظار preview:
- `entryPoint`: `Run`
- `functions`: شامل `Nosazi` (supported) و `iNcOME` (unsupported)
- `parseSuccess`: true

### ۳) تأیید DB

```sql
USE RayvarzRuleEngine;
SELECT TOP 5 SnapshotId, DslVersion, XmlHash, ParserVersion, IsActive, LEN(DslJson) AS JsonLen
FROM dbo.RuleDslSnapshot
WHERE NidMember = 1388
ORDER BY DslVersion DESC;
```

`IsActive` باید `0` باشد تا فاز ۴.

## ساختار کد

```
RuleEngine/Parser/
  DslModel.cs           — AST types
  XmlEnvelopeReader.cs  — ClsFunction + canonical hash
  VbFunctionExtractor.cs
  VbStatementParser.cs
  VbTranspiler.cs
  RuleDslParserService.cs
  Fixtures/member-1388-sample.xml
```

## Sync خودکار

`RuleVersionManager` هنگام تشخیص candidate جدید در `MemberHistory`:
1. `RuleCandidate` insert
2. parse → `RuleDslSnapshot`
3. status → `Parsed` یا `Rejected`

## فاز بعدی

فاز ۳: `IOperationRegistry` + `DslExecutor` + `DynamicRuleEngine` (اجرای AST).

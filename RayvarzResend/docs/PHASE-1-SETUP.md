# فاز ۱ — Sara live یکپارچه + IFicheRuleEngine

پیش‌نیاز: فاز ۰ (`PHASE-0-SETUP.md`) — DB `RayvarzRuleEngine` و ۴ golden seed.

## هدف فاز ۱

| # | کار | وضعیت |
|---|-----|--------|
| 1.1 | `FicheRepository` تنها منبع فیش (Duty_Fiche + Sub از Sara8M03 live) | ✅ |
| 1.2 | `GoldenDryRunService` از `IFicheRuleEngine` | ✅ |
| 1.3 | `LegacyRuleEngine` = `DutyNosaziLogic` + `SoapBuilder` | ✅ |
| 1.4 | مستندات connection سرور ۲۳۲ | این سند |

**تعریف done:** `POST /api/rule/golden/dry-run` → `passed: 4, allPassed: true`؛ SOAP خروجی همان v16.

## معماری فاز ۱

```
Sara8M03 (live) ──► FicheRepository ──► FicheHeaderDto + Rows (DutyNosaziLogic)
                                              │
                                              ▼
                              RuleEngineFactory.ResolveAsync()
                                              │
                         ┌────────────────────┴────────────────────┐
                         ▼                                         ▼
                 LegacyRuleEngine                           DynamicRuleEngine
            (SoapBuilder + validate rows)                    (stub — فاز ۳)
                         │
                         ▼
              RayvarzPayloadBuilder / GoldenDryRunService
```

- **ActiveEngine** از `RayvarzRuleEngine.dbo.RuleSyncState` خوانده می‌شود (پیش‌فرض: `Legacy`).
- **تولید:** `Rayvarz:PayloadSource` = `LegacyCSharp` (بدون تغییر نسبت به v16).
- **Dynamic:** تا فاز ۴ promote نمی‌شود؛ stub فقط برای تست `ForceEngine`.

## Connection strings — سرور ۲۳۲

در `appsettings.Development.json` (خارج git):

```json
{
  "ConnectionStrings": {
    "Sara": "Server=232;Database=Sara8M03;User Id=...;Password=...;TrustServerCertificate=True;",
    "Rayvarz": "Server=232;Database=Ray_CityHall;User Id=...;Password=...;TrustServerCertificate=True;",
    "RuleEngine": "Server=232;Database=DbRuleEngein;User Id=...;Password=...;TrustServerCertificate=True;",
    "RayvarzRuleEngine": "Server=232;Database=RayvarzRuleEngine;User Id=...;Password=...;TrustServerCertificate=True;"
  },
  "Rayvarz": {
    "PayloadSource": "LegacyCSharp",
    "DryRun": true
  },
  "RuleEngine": {
    "NidMemberRayvarzRun": 1388,
    "ForceEngine": ""
  }
}
```

| Connection | Database | نقش |
|------------|----------|-----|
| `Sara` | **Sara8M03** | فیش live — `Duty_Fiche`, `Duty_FicheSub`, `Income_Fiche` |
| `Rayvarz` | **Ray_CityHall** | چک تکراری / `ray.incmdocsys` |
| `RuleEngine` | **DbRuleEngein** | `Member` + `MemberHistory` (read-only) |
| `RayvarzRuleEngine` | **RayvarzRuleEngine** | golden، sync state، dry-run log |

**اشتباه رایج:** `RayvarzRuleEngine` را روی `DbRuleEngein` گذاشتن.

## API جدید / به‌روز

| Endpoint | کار |
|----------|-----|
| `GET /api/rule/engine` | موتور فعال (`ActiveEngine` + نام resolve‌شده) |
| `POST /api/rule/golden/dry-run` | حالا `engineName` از factory برمی‌گردد |
| `POST /api/fiche/preview` | فیلد `engineName` در پاسخ |

## تست

```bash
# 1) اتصال‌ها
curl http://localhost:5123/api/db-test

# 2) موتور فعال
curl http://localhost:5123/api/rule/engine

# 3) golden dry-run (باید 4/4 سبز)
curl -X POST http://localhost:5123/api/rule/golden/dry-run
```

پاسخ مورد انتظار dry-run:

```json
{
  "engineName": "LegacyCSharp",
  "total": 4,
  "passed": 4,
  "allPassed": true
}
```

## ForceEngine (فقط dev)

برای تست stub Dynamic بدون تغییر DB:

```json
"RuleEngine": { "ForceEngine": "Dynamic" }
```

ارسال SOAP با Dynamic ناموفق است و به Legacy fallback می‌شود (در `RayvarzPayloadBuilder`).

## فاز بعدی

فاز ۲: Parser `XmlBody` → AST (`Run` + `Nosazi`) — بدون تغییر مسیر تولید.

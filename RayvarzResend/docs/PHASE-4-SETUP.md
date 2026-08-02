# فاز ۴ — Version Manager + Promote + Circuit Breaker

پیش‌نیاز: فاز ۳ (Executor + DynamicRuleEngine).

## هدف

| # | کار | وضعیت |
|---|-----|--------|
| 4.1 | State machine Candidate → Validated → DryRunPassed → Promoted | ✅ |
| 4.2 | شرط Promote: validation + golden 4/4 + stability + hash match | ✅ |
| 4.3 | `ActiveEngine=Dynamic` فقط بعد از promote | ✅ |
| 4.4 | Circuit breaker → Legacy در خطای متوالی Dynamic | ✅ |
| 4.5 | Rollback دستی با `POST /api/rule/promote/rollback` | ✅ |

**تعریف done:** تغییر آزمایشی در MemberHistory → بعد از 72h + تست‌ها → Active Dynamic؛ rollback با یک API.

## State machine

```
Detected → Parsing → Parsed → Validated → DryRunPassed → Promoted
                ↓         ↓          ↓              ↓
            Rejected  Rejected   Rejected       (ActiveEngine=Dynamic)
```

## شرط‌های Promote

| شرط | توضیح |
|-----|--------|
| Parse + Validation | AST بدون operation ناشناخته |
| Golden dry-run | ۴/۴ با `allowLegacyFallback=false` |
| Stability | `StableEligibleAtUtc` گذشته باشد (`StabilityHours` پیش‌فرض 72) |
| Hash match | `CanonicalXmlHash` = hash فعال `Member.XmlBody` |
| No newer history | آخرین `MemberHistory` همان `SourceNidHistory` candidate |

## Migration SQL (اختیاری)

روی `RayvarzRuleEngine`:

```sql
-- database/03_Phase4_CircuitBreaker.sql
```

ستون‌های `ConsecutiveDynamicFailures` و `CircuitBreakerOpenUntilUtc` در `RuleSyncState`.

## Config

```json
"RuleEngine": {
  "StabilityHours": 72,
  "EnableAutoPromote": false,
  "CircuitBreakerFailureThreshold": 3,
  "CircuitBreakerCooldownMinutes": 60,
  "ForceEngine": ""
}
```

| کلید | پیش‌فرض | نقش |
|------|---------|-----|
| `EnableAutoPromote` | `false` | promote خودکار در background poll |
| `StabilityHours` | `72` | ساعت انتظار پایداری؛ برای dev می‌توان `0` گذاشت |
| `CircuitBreakerFailureThreshold` | `3` | خطای متوالی Dynamic قبل از بازگشت به Legacy |
| `CircuitBreakerCooldownMinutes` | `60` | مدت غیرفعال بودن Dynamic بعد از breaker |

**تولید:** `EnableAutoPromote=false` و `PayloadSource=LegacyCSharp` تا زمانی که promote دستی تأیید شود.

## API

| Endpoint | Method | کار |
|----------|--------|-----|
| `/api/rule/promote/status` | GET | وضعیت ActiveEngine، candidates، logs |
| `/api/rule/promote/run` | POST | اجرای promote (`?force=true` بدون انتظار 72h) |
| `/api/rule/promote/rollback` | POST | `ActiveEngine=Legacy` + deactivate snapshots |

### نمونه PowerShell (پورت 5000)

```powershell
# وضعیت
Invoke-RestMethod -Uri "http://localhost:5000/api/rule/promote/status"

# promote دستی (بدون EnableAutoPromote)
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/promote/run?force=true"

# rollback
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/promote/rollback" `
  -ContentType "application/json" -Body '{"reason":"test rollback"}'
```

## جریان تست dev

1. `POST /api/rule/sync/run` — sync MemberHistory (+ retry parse برای candidateهای گیرکرده در Detected)
2. `POST /api/rule/dsl/parse` — snapshot DSL + لینک candidate به Parsed
3. `GET /api/rule/promote/status` — بررسی candidate (باید `Parsed` یا بالاتر باشد)
4. `StabilityHours: 0` در Development یا `POST .../promote/run?force=true`
5. `GET /api/rule/engine` — `activeEngine=Dynamic` بعد از promote
6. `POST /api/rule/golden/dry-run` — تأیید ۴/۴
7. در صورت مشکل: `POST /api/rule/promote/rollback`

### عیب‌یابی: candidate در `Detected` مانده

اگر `promote/run` می‌گوید «No candidates» و status هنوز `Detected` است:

```powershell
# دوباره sync (parse گیرکرده را retry می‌کند)
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/sync/run"

# یا dsl/parse (candidate را به Parsed لینک می‌کند)
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/dsl/parse"

# سپس promote
Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/rule/promote/run?force=true"
```

## Circuit breaker

- هر خطای واقعی Dynamic (بدون fallback موفق) → `ConsecutiveDynamicFailures++`
- بعد از threshold → `ActiveEngine=Legacy` + cooldown
- موفقیت Dynamic → reset شمارنده
- `ForceEngine=Dynamic` در زمان breaker باز → همچنان Legacy

## Background sync

هر `PollIntervalMinutes` (پیش‌فرض ۱۵):

1. `RuleVersionManager.EvaluateChangesAsync` — detect + parse
2. `RulePromotionService.EvaluatePromotionsAsync` — فقط اگر `EnableAutoPromote=true`

# Rule Engine — Sync و Version Manager

پیاده‌سازی **فاز ۰.۵ + اسکلت فاز ۴** (شاخه `cursor/rule-engine-sync-phase0-4-4a`).

## فعال‌سازی

1. اسکریپت `database/11_RuleEngine_Sync_Schema.sql` را روی **RayvarzRuleEngine** اجرا کنید (یا اجازه دهید اپ جداول را در اولین sync بسازد).
2. در `appsettings.json`:

```json
"RuleEngineSync": {
  "Enabled": true,
  "PollIntervalMinutes": 15,
  "StabilityHours": 72,
  "NidMember": 1388,
  "AutoPromoteDynamic": false,
  "StateConnectionStringName": "AppAuth"
}
```

3. `ConnectionStrings:RuleEngine` → **DbRuleEngein** (خواندن `Member` / `MemberHistory`).
4. `ConnectionStrings:AppAuth` → **RayvarzRuleEngine** (ذخیره state).

## چرخه کار

هر `PollIntervalMinutes` (پیش‌فرض ۱۵ دقیقه):

1. خواندن `Member` فعال → **هش canonical** `XmlBody`.
2. مقایسه با `RuleSyncState.ActiveXmlBodyHash` و `LastMemberXmlBodyHash`.
3. خواندن آخرین `MemberHistory` → مقایسه `NidHistory` با `LastSeenNidHistory`.
4. در صورت تغییر → `RuleCandidate` با state `Detected` → parse (`ClsFunctionParser`) → `Validated`.
5. پس از **StabilityHours** (۷۲h) بدون History جدیدتر → dry run golden (placeholder) → `PromoteReady`.
6. اگر `AutoPromoteDynamic=true` → `ActiveEngine=Dynamic` و log در `RulePromotionLog`؛ در غیر این صورت فقط log و hash فعال به‌روز می‌شود و **ارسال تولید همچنان `LegacyCSharp`**.

## API (ادمین)

| متد | مسیر |
|-----|------|
| GET | `/api/rule/sync/status` |
| POST | `/api/rule/sync/run` — اجرای دستی یک چرخه |

## آنچه هنوز نیست (فاز ۲–۵)

- DSL snapshot کامل در `RuleDslSnapshot`
- Dry run سطح B مقابل `ray.incmdocsys`
- `DynamicRuleEngine` و تغییر خودکار `Rayvarz:PayloadSource`

تا آن زمان `Rayvarz:PayloadSource` را **LegacyCSharp** نگه دارید.

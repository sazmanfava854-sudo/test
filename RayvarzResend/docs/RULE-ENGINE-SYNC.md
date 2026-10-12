# Rule Engine — Sync و Version Manager

پیاده‌سازی **فاز ۰.۵ + اسکلت فاز ۴** (شاخه `cursor/rule-engine-sync-phase0-4-4a`).

## فعال‌سازی

1. جداول `RuleSyncState`, `RuleCandidate`, … باید از قبل روی **RayvarzRuleEngine** با ستون‌های مرجع در `database/11_RuleEngine_Sync_Schema.sql` وجود داشته باشند. اپ جدول جدید با نام‌های متفاوت نمی‌سازد.
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
2. مقایسه هش `XmlBody` عضو فعال با `RuleSyncState.LastStableXmlHash`.
3. خواندن آخرین `MemberHistory` → مقایسه `NidHistory` با `LastSeenNidHistory`.
4. در صورت تغییر → `RuleCandidate` با `Status=PendingStability` و `StableEligibleAtUtc = now + StabilityHours`.
5. پس از رسیدن `StableEligibleAtUtc` → dry run golden (placeholder).
6. اگر `AutoPromoteDynamic=true` → `ActiveEngine=Dynamic` و log در `RulePromotionLog`؛ در غیر این صورت فقط log و hash فعال به‌روز می‌شود و **ارسال تولید همچنان `LegacyCSharp`**.

## API (ادمین)

| متد | مسیر |
|-----|------|
| GET | `/api/rule/sync/status` |
| POST | `/api/rule/sync/run` — **همین الان** یک چرخه (بعد از تغییر Member/History) |

با `Enabled: true` اولین چرخه **بلافاصله بعد از استارت** اپ هم اجرا می‌شود (بدون انتظار ۱۵ دقیقه).

### شروع فوری بعد از تغییر دیتابیس

1. تغییر باید روی **DbRuleEngein** باشد: `dbo.Member` (XmlBody فعال) یا ردیف جدید در `dbo.MemberHistory` برای `NidMember=1388`.  
   تغییر روی `RayvarzRuleEngine` / `AppUser` توسط این sync **دیده نمی‌شود**.
2. در `appsettings.json`: `"RuleEngineSync": { "Enabled": true }`
3. یکی از این دو:
   - **ری‌استارت** سرویس (چرخه اول خودکار)، یا
   - بدون ری‌استارت: با کوکی ادمین → `POST /api/rule/sync/run`
4. نتیجه: `GET /api/rule/sync/status` → `openCandidate` / `state.lastStableXmlHash`

## آنچه هنوز نیست (فاز ۲–۵)

- DSL snapshot کامل در `RuleDslSnapshot`
- Dry run سطح B مقابل `ray.incmdocsys`
- `DynamicRuleEngine` و تغییر خودکار `Rayvarz:PayloadSource`

تا آن زمان `Rayvarz:PayloadSource` را **LegacyCSharp** نگه دارید.

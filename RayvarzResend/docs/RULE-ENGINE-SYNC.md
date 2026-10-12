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
  "StateConnectionStringName": "AppAuth",
  "SendGateEnabled": true,
  "SendGateCacheSeconds": 15
}
```

3. `ConnectionStrings:RuleEngine` → **DbRuleEngein** (خواندن `Member` / `MemberHistory`).
4. `ConnectionStrings:AppAuth` → **RayvarzRuleEngine** (ذخیره state).

## چرخه کار

هر `PollIntervalMinutes` (پیش‌فرض ۱۵ دقیقه) — اجراهای هم‌زمان (پس‌زمینه + `POST /run`) با قفل سریال می‌شوند:

1. خواندن `Member` فعال → **هش canonical** `XmlBody` (منبع حقیقت محتوا).
2. خواندن آخرین `MemberHistory` → `LastSeenNidHistory` / `LastSeenModifyAt` (تاریخ شمسی `ModifyDate` به میلادی تبدیل می‌شود).
3. تصمیم:

| وضعیت | اقدام |
|-------|-------|
| `LastStableXmlHash` خالی (اولین اجرا) | **Baseline**: نسخه فعلی پایه می‌شود، بدون انتظار ۷۲ ساعت |
| هش = `LastStableXmlHash` | بدون تغییر؛ candidateهای در انتظار → `Rejected` (برگشت به نسخه پایدار) |
| هش = candidate باز | فقط `LastCheckedAtUtc` به‌روز می‌شود |
| هش جدید | candidate `Detected` با `StableEligibleAtUtc = now + StabilityHours`؛ candidateهای قبلی → `Rejected` (Superseded). اگر همین هش قبلاً دیده شده (UNIQUE)، همان ردیف دوباره باز می‌شود |

4. XmlBody نامعتبر (parse ناموفق با `ClsFunctionParser`) همان لحظه `Rejected` می‌شود و تا تغییر محتوا دوباره باز نمی‌شود.
5. پس از رسیدن `StableEligibleAtUtc` و parse موفق → `LastStable*` به نسخه جدید به‌روز می‌شود:
   - `AutoPromoteDynamic=false` (پیش‌فرض): candidate → `Stable`، `ActiveEngine` همان `Legacy`.
   - `AutoPromoteDynamic=true` و circuit breaker بسته: candidate → `Promoted`، `ActiveEngine=Dynamic`.
6. همه رویدادها در `RulePromotionLog.Action`: `Baseline`, `Superseded`, `Stable`, `Promoted`, `Rejected`, `SendBlocked`, `SendApproved`, `SendReopened`.

## SendGate — بستن ارسال از لحظه Detected

ستون‌ها: `database/12_RuleEngine_SendGate.sql` (اپ در اولین اتصال خودش اضافه می‌کند؛ اگر مجوز ALTER نیست اسکریپت را دستی اجرا کنید).

| رویداد | `RuleSyncState.SendGate` |
|--------|--------------------------|
| Baseline (اولین اجرا) | `Open`، `ApprovedSendXmlHash` = هش فعلی |
| هش `Member` ≠ `ApprovedSendXmlHash` (همان لحظه Detected، حتی اگر XmlBody نامعتبر باشد) | `Blocked` + `SendBlocked` در log |
| مدیر «تأیید به‌روزرسانی» (`POST /api/rule/send-gate/approve`) | `Open`، `ApprovedSendXmlHash` = هش فعلی `Member` + `SendApproved` |
| `Member` به همان نسخه تأییدشده برگردد | `Open` + `SendReopened` |
| پایان ۷۲ ساعت (`Stable`) | **بدون اثر** روی Gate — فقط audit |

وقتی `Blocked` است، `/api/fiche/send`، `/api/tahator/send` و `/api/unsent/send-batch` با HTTP 409 و فقط این پیام رد می‌شوند:

```
نیاز به به روز رسانی هست
```

روال مدیر: لاگ هشدار `sending to Rayvarz is BLOCKED` یا کارت «به‌روزرسانی قوانین» در `/management/` → اعمال تغییر در کد و deploy → دکمه «تأیید به‌روزرسانی و باز کردن ارسال». (`POST /api/rule/send-gate/block` بستن دستی.)

- `SendGateEnabled=false` گیت را غیرفعال می‌کند؛ با `Enabled=false` (sync خاموش) هم گیت اعمال نمی‌شود.
- خطای اتصال به RayvarzRuleEngine هنگام خواندن Gate ارسال را متوقف نمی‌کند (هشدار در لاگ).
- مقدار Gate حداکثر `SendGateCacheSeconds` (پیش‌فرض ۱۵ ثانیه) کش می‌شود؛ تأیید/بستن از API کش را فوراً پاک می‌کند.

ارسال تولید در هر حال **`LegacyCSharp`** می‌ماند؛ هیچ کدی هنوز `ActiveEngine` را برای ساخت سند نمی‌خواند.
Dry run golden (`RuleDryRunResult`) هنوز وصل نیست و sync ردیف ساختگی در آن نمی‌نویسد.

## API (ادمین)

| متد | مسیر |
|-----|------|
| GET | `/api/rule/sync/status` |
| POST | `/api/rule/sync/run` — **همین الان** یک چرخه (بعد از تغییر Member/History) |
| POST | `/api/rule/send-gate/approve` — بدنه اختیاری `{ "note": "..." }` |
| POST | `/api/rule/send-gate/block` — بدنه اختیاری `{ "note": "..." }` |

با `Enabled: true` اولین چرخه **بلافاصله بعد از استارت** اپ هم اجرا می‌شود (بدون انتظار ۱۵ دقیقه).

### شروع فوری بعد از تغییر دیتابیس

1. تغییر باید روی **DbRuleEngein** باشد: `dbo.Member.XmlBody` فعال برای `NidMember=1388` (ردیف جدید `MemberHistory` بدون تغییر `XmlBody` فقط `LastSeenNidHistory` را جلو می‌برد).  
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

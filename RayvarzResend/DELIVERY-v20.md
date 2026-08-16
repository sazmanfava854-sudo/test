# تحویل نهایی — RayvarzResend نسخه ۲۰

این نسخه **تمام باگ‌های شناسایی‌شده** در زنجیره توسعه را شامل می‌شود (به‌جز باگ‌های ۲ و ۱۶ که باگ واقعی نیستند).

## دریافت

| روش | مسیر |
|-----|------|
| Zip ثابت | `RayvarzResend-20.zip` (ریشه مخزن، شاخه تحویل) |
| Git | `cursor/rayvarz-resend-delivery-v20-59b2` → merge به `rayvarz-resend` |
| تگ پیشنهادی | `rayvarz-resend-v20` |

پس از اجرا: `GET /api/config` → `releaseVersion: 20`

## باگ‌های برطرف‌شده

| # | موضوع | وضعیت |
|---|--------|--------|
| 1 | Income SOAP parity (Bank, Ref, RefRowDocNo, IncmMkrTyp) | ✅ |
| 2 | — | ⏭️ باگ نیست |
| 3 | اعتبارسنجی سمت سرور قبل از ارسال | ✅ |
| 4 | مسدودسازی ارسال وقتی چک تکراری Rayvarz خطا می‌دهد | ✅ |
| 5 | حذف fallback ردیف IncmNo=0 | ✅ |
| 6 | Fund شعبه ۲۱۲ مطابق nosazo.vb | ✅ |
| 7 | ActDate درآمد از وضعیت فیش | ✅ |
| 8 | Oddment (Income/Duty) | ✅ |
| 9 | تهاتر ۱۵۷+۱۵۸ — جفت، SOAP، API، snapshot | ✅ |
| 10 | Income_Calculation scale به Payable | ✅ (member1388 parity) |
| 11 | — | ✅ (در PR #47) |
| 12 | ResetStatus پیش‌فرض false | ✅ |
| 13 | Success=true وقتی SOAP OK ولی verify شکست خورد | ✅ |
| 14 | UI — جمع Val ارسالی در برابر Payable | ✅ |
| 15 | تاریخ شمسی بدون PadLeft ناامن | ✅ |
| 16 | — | ⏭️ باگ نیست |
| 17 | هشدار GetNosaziNickName SQL failure | ✅ |
| 18 | RuleEngineBridge stub محلی | ✅ |
| 19 | import تست‌های bug1-tests به پروژه | ✅ |
| 20 | Golden seed تهاتر ۱۵۷ (IDs 11–14) | ✅ |

## تست قبل از تحویل

```bash
cd RayvarzResend
dotnet test                    # 162 تست — همه باید Pass
node scripts/Bug14ValSummaryTests.mjs   # 7 تست UI
```

## اجرا

```bash
cd RayvarzResend/RayvarzResend.Web
dotnet run
```

اول `Rayvarz:DryRun=true` — سپس فیش غیرتکراری — سپس `DryRun=false`.

## یادداشت‌های باقی‌مانده (خارج از scope باگ)

- Golden مسیر **۱۵۸ / درآمدی** (IDs 15+) — منتظر export از `incmdocsys`
- SaraBridge واقعی روی سرور Sara — برای `PayloadSource=RuleEngineBridge` در production
- Rule Engine Phase 5 — golden level-B کامل

## Baseline قبلی

برای بازگشت به نسخه C# ثابت بدون Rule Engine: `BASELINE-v16.md` و `baseline/rayvarz-resend-v16`.

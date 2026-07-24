# Patch پیشنهادی Q05_A — گیت‌وی LoRa با نصب آسان

فایل اصلی **`IoTRecommendation.Web/Data/Questions.json`** عمداً **تغییر نکرده** است.

نسخهٔ کامل جایگزین (فقط تفاوت در گزینهٔ **Q05_A**):

`IoTRecommendation.Web/Data/Questions.variant-Q05A-lorawan-gateway.json`

---

## مشکل در نسخهٔ فعلی

گزینه **Easy (Q05_A)** در `Questions.json`:

- `HardwareCAPEX` و `AnnualConnectivityOPEX` را **کم‌اهمیت** می‌کند (`delta: -1`).
- برای سناریوی **LoRa + گیت‌وی محلی + OPEX محدود** (مثلاً Q11 = Very limited)، وزن OPEX کم می‌شود و **مزیت LoRa نسبت به NB-IoT** در TOPSIS محو می‌شود؛ در شبیه‌سازی با Q05=A اغلب LoRaWAN رتبهٔ ۱ نمی‌شود.

---

## منطق پچ

**نصب آسان گیت‌وی** = امکان **شبکهٔ اختصاصی LPWAN** روی سایت، نه «هزینهٔ سخت‌افزار و اشتراک کم‌اهمیت است».

| معیار | delta جدید | توجیه |
|--------|------------|--------|
| `CellularSupport` | **−1** | وابستگی به اپراتور سلولار کمتر (مثل قبل). |
| `TransmissionRange` | **+1** | گیت‌وی قابل نصب → پوشش برد مهم‌تر. |
| `LinkBudget` | **+1** | لینک رادیویی / پوشش برای LPWAN مهم‌تر. |
| ~~HardwareCAPEX~~ | حذف | بودجهٔ CAPEX در Q10 جدا پرسیده می‌شود. |
| ~~AnnualConnectivityOPEX~~ | حذف | تحمل OPEX در Q11 جدا پرسیده می‌شود. |

**Q05_B** و **Q05_C** بدون تغییر (مثل فایل اصلی).

---

## Diff (فقط Q05_A)

```diff
         "id": "Q05_A",
         "text": "Easy",
         "effects": [
-          { "criterionKey": "HardwareCAPEX",            "delta": -1 },
-          { "criterionKey": "AnnualConnectivityOPEX",   "delta": -1 },
-          { "criterionKey": "CellularSupport",          "delta": -1 }
+          { "criterionKey": "CellularSupport",          "delta": -1 },
+          { "criterionKey": "TransmissionRange",       "delta": 1 },
+          { "criterionKey": "LinkBudget",               "delta": 1 }
         ]
```

---

## فعال‌سازی در برنامه (بدون دست‌کاری Questions.json)

در `IoTRecommendation.Web/appsettings.Development.json` (یا موقت در `appsettings.json`):

```json
"DataPaths": {
  "QuestionsFile": "Questions.variant-Q05A-lorawan-gateway.json"
}
```

سپس برنامه را restart کنید و دوباره **Run Clustering** → خوشه LPWAN → پرسشنامه.

برای بازگشت به نسخهٔ پایان‌نامه/پیش‌فرض:

```json
"QuestionsFile": "Questions.json"
```

---

## سناریوی نمونه (با این variant)

- خوشه: **Long-Range LPWAN**
- Q05 = **Easy**, Q06 = Poor cellular, Q01/Q08 بزرگ/برد زیاد, Q09 کم, Q11 OPEX محدود, Q13 نزدیک real-time

در شبیه‌سازی با وزن‌های AHP فعلی، یک پروفایل نمونه **LoRaWAN** را در TOPSIS جلو انداخت (رتبه‌های نزدیک به NB-IoT).

---

## پایان‌نامه

می‌توانید این را به‌عنوان **نسخهٔ حساسیت / هم‌ترازی دامنه‌ای** گزارش کنید و `Questions.json` را به‌عنوان baseline نگه دارید.

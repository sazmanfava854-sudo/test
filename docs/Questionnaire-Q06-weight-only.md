# Q06 — بدون سلولار، فقط با وزن (بدون حذف در کد)

فایل‌های **`Questions.json`** و **`Settings.json` اصلی** را خودتان ویرایش کنید.  
کد برنامه **فناوری را حذف نمی‌کند**؛ فقط وزن‌های تطبیقی و نوع معیار در TOPSIS/VIKOR/COPRAS اثر دارد.

---

## چرا قبلاً NB-IoT با Q06=C برنده می‌شد؟

1. **`CellularSupport` نوع Benefit بود** → مقدار **۱ بهتر** از ۰ → NB-IoT برندهٔ این معیار است.
2. **`delta: -1`** یعنی **اهمیت معیار کمتر**، نه «سلولار ممنوع» → NB با برد/انرژی می‌تواند کل رتبه را ببرد.
3. **Q06_A و Q06_C هر دو −1** بودند (اشتباه معنایی).

---

## گام ۱ — `Settings.json` (یک معیار)

`CellularSupport` را به **Cost** تغییر دهید (وابستگی به مودم/شبکهٔ سلولار = **هزینه/عیب**، نه مزیت):

```json
{
  "key": "CellularSupport",
  "displayName": "Cellular dependency (operator modem)",
  "unit": "0/1",
  "type": "Cost",
  "usedInClustering": false,
  "usedInTopsis": true,
  "transform": "None"
}
```

**تفسیر داده:** NB-IoT → `1` (وابستگی بالا)، LoRaWAN/Sigfox → `0` (بدون سلولار).

> **AHP:** ماتریس‌های expert برای سلولار با معیار Benefit ساخته شده‌اند. اگر نوع را Cost می‌کنید، در فصل روش‌شناسی بنویسید معیار در TOPSIS به‌صورت «هزینهٔ وابستگی به اپراتور» مدل شده است؛ یا ماتریس AHP را با داور هماهنگ کنید.

---

## گام ۲ — `Questions.json` (فقط بلوک Q06)

```json
{
  "id": "Q06",
  "order": 6,
  "text": "What is the cellular network availability at the project site?",
  "options": [
    {
      "id": "Q06_A",
      "text": "Good coverage and allowed usage",
      "effects": [
        { "criterionKey": "CellularSupport", "delta": -1 }
      ]
    },
    {
      "id": "Q06_B",
      "text": "Moderate coverage or minor restrictions",
      "effects": []
    },
    {
      "id": "Q06_C",
      "text": "Poor coverage or cellular not possible",
      "effects": [
        { "criterionKey": "CellularSupport", "delta": 3 }
      ]
    }
  ]
}
```

**حداقل پیکربندی (همان چیزی که در `Questions.json` commit شده):** فقط `CellularSupport` با **+3** در Q06_C. برای نزدیک‌تر شدن رتبهٔ **VIKOR** به TOPSIS/COPRAS می‌توانید اختیاری اضافه کنید:

```json
{ "criterionKey": "RTTLatency", "delta": 2 },
{ "criterionKey": "AnnualConnectivityOPEX", "delta": 1 }
```

| گزینه | delta | با نوع **Cost** |
|--------|--------|------------------|
| **A** پوشش خوب | **−1** | وزن «وابستگی سلولار» **کمتر** |
| **B** متوسط | خالی | فقط وزن پایه AHP |
| **C** بدون سلولار | **+3 سلولار** (اختیاری: +2 RTT، +1 OPEX) | جریمهٔ NB در TOPSIS/COPRAS؛ VIKOR ممکن است به تقویت RTT نیاز داشته باشد |

`CellularSupport` فقط **+1** (مثلاً اگر Q05_A هم `Cellular −1` بدهد) برای **TOPSIS** کافی است؛ برای **VIKOR** معمولاً **کافی نیست** — به **+3** برسانید یا RTT را تقویت کنید.

---

## چرا VIKOR ممکن است NB را رتبه ۲ بگذارد (با Q06=C)؟

این **سلولار را ترجیح نمی‌دهد**. در خروجی شما:

- **Sigfox** و **NB-IoT** هر دو **R ≈ 0.181**
- **LoRaWAN** **R ≈ 0.219** (بدترین «پشیمانی» روی **یک** معیار)

با **Si = +1 روی Energy** (مثلاً Q12 طول عمر باتری بالا)، VIKOR روی **مصرف انرژی** LoRa را سخت‌تر از NB جریمه می‌کند → LoRa رتبه **۳**، NB رتبه **۲** — حتی وقتی سلولار Cost است و NB رتبه **۳** در TOPSIS است.

**کارهایی که خودتان اعمال کنید (بدون تغییر کد):**

1. **Q06_C** طبق fragment بالا: `CellularSupport +3` و `RTTLatency +2` (و در صورت نیاز OPEX +1).
2. **Q05_A** در variant قدیمی `Cellular −1` دارد — با Q06=C **خنثی** می‌شود؛ یا Q05 را **B** بگذارید، یا variant بدون `Cellular` روی Easy.
3. برای نزدیک کردن VIKOR به TOPSIS: **Q12** را **B** (نه C با Energy +1) یا **Q13=B** برای RTT؛ انرژی خیلی بالا نبرید مگر هدف شما Sigfox باشد.
4. بعد از Submit، **Adaptive** را چک کنید: `CellularSupport` حدود **≥ 0.25–0.35** (با Si=+3)؛ `RTT` هم بالاتر از ~0.12.

شبیه‌سازی با همان داده‌ها: با **Si سلولار ≥ +2** و بدون فشار اضافهٔ انرژی، VIKOR معمولاً **NB را رتبه ۳** می‌دهد.

---

## گام ۳ — بقیهٔ پرسشنامه (برای LPWAN بدون سلولار)

هم‌زمان با Q06=C معمولاً:

| سوال | پیشنهاد |
|------|---------|
| Q05 | A (نصب آسان گیت‌وی) — در variant فقط Cellular |
| Q11 | A (OPEX محدود) — LoRa بهتر از NB |
| Q08 | C (فاصله زیاد) — در صورت نیاز |

بعد از ذخیره فایل‌ها: **پرسشنامه را دوباره Submit** کنید و در Results ستون **Adaptive** را ببینید:  
`CellularSupport` (Cost) باید **وزن بالا** داشته باشد.

---

## جملهٔ پایان‌نامه (نسخهٔ وزن‌محور)

«در صورت عدم امکان استفاده از شبکهٔ سلولار (Q06_C)، اهمیت معیار وابستگی سلولار (CellularSupport به‌عنوان معیار هزینه) در وزن‌های تطبیقی افزایش می‌یابد و فناوری‌های با مقدار بالاتر این معیار در MCDM تنزل می‌یابند، بدون حذف سخت از فضای گزینه‌ها.»

---

## فایل نمونهٔ کامل (اختیاری)

`IoTRecommendation.Web/Data/Questions.patch-Q06-weight-only.fragment.json` — فقط بلوک Q06 برای کپی.

`docs/Settings.snippet-CellularSupport-as-Cost.json` — فقط یک معیار برای کپی در `Settings.json`.

---

## حذف حذف صریح از کد

سرویس `QuestionnaireEligibilityService` از پروژه برداشته شد؛ فقط این رویکرد وزن‌محور را اعمال کنید.

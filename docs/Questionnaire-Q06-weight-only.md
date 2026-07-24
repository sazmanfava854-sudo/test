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
        { "criterionKey": "CellularSupport", "delta": 2 }
      ]
    }
  ]
}
```

| گزینه | delta | با نوع **Cost** |
|--------|--------|------------------|
| **A** پوشش خوب | **−1** | وزن «وابستگی سلولار» **کمتر** → راه‌حل‌های سلولار کمتر جریمه می‌شوند |
| **B** متوسط | خالی | فقط وزن پایه AHP |
| **C** بدون سلولار | **+2** | وزن «وابستگی سلولار» **بیشتر** → NB-IoT (`1`) در TOPSIS/COPRAS **شدیدتر جریمه** می‌شود |

`delta: 2` اختیاری است؛ `+1` هم کار می‌کند، `+2` با `α=0.5` قوی‌تر است.

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

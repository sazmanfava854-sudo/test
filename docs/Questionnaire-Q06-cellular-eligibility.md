# Q06 — سلولار و حذف NB-IoT

## مشکل قبلی (فقط وزن تطبیقی)

`delta: -1` روی `CellularSupport` یعنی **اهمیت معیار سلولار در وزن کمتر می‌شود**، نه «فناوری سلولار ممنوع است».

در `Questions.json` اصلی، **Q06_A** و **Q06_C** هر دو `CellularSupport -1` داشتند — از نظر معنایی اشتباه بود.

با وزن کم، NB-IoT هنوز می‌توانست با **برد، انرژی، …** در TOPSIS/COPRAS برنده شود.

## رفتار جدید (از commit مربوط به `QuestionnaireEligibilityService`)

اگر کاربر **Q06_C** («Poor coverage or cellular not possible») را انتخاب کند:

- قبل از Phase 4، هر فناوری با `CellularSupport ≥ 0.5` (مثلاً **NB-IoT**) از لیست **حذف** می‌شود.
- TOPSIS / VIKOR / COPRAS فقط روی **LoRaWAN و Sigfox** (در خوشه LPWAN) اجرا می‌شوند.
- در صفحه Results، بخش **Questionnaire eligibility** فناوری‌های حذف‌شده را نشان می‌دهد.

`Questions.json` اصلی **تغییر نکرده**؛ قانون حذف در **کد** است.

## variant پرسشنامه (اختیاری)

در `Questions.variant-Q05A-lorawan-gateway.json`:

- **Q06_A:** `CellularSupport +1` (پوشش خوب → اهمیت معیار سلولار بیشتر)
- **Q06_C:** `effects: []` (حذف فقط از طریق eligibility)

## پایان‌نامه

- **وزن تطبیقی (Si):** اهمیت معیارها  
- **Eligibility (Q06_C):** منع سخت فناوری‌های سلولار

# انتخاب بهینه فناوری IoT — پاک‌سازی و نرمال‌سازی داده

پروژه پشتیبان پژوهش: **K-Means → AHP → TOPSIS**

## ساختار پوشه‌ها

```
data/
  criteria_config.json                  ← تعریف پروفایل‌های معیار
  raw/iot_technologies_raw.csv
  processed/iot_technologies_clean_core.csv
  processed/iot_technologies_clean_extended.csv
  processed/iot_technologies_normalized_core.csv
  processed/iot_technologies_normalized_extended.csv
scripts/
  clean_and_normalize.py
CRITERIA_GUIDE.md                       ← راهنمای کامل معیارها
```

## اجرا

```bash
pip install -r requirements.txt
python3 scripts/clean_and_normalize.py --profile all
```

پروفایل‌ها:
- `core` — ۸ معیار (مطابق پروپوزال)
- `extended` — ۱۴ معیار (توسعه‌یافته)
- `all` — هر دو (پیش‌فرض)

جزئیات معیارها: [CRITERIA_GUIDE.md](CRITERIA_GUIDE.md)

## ۸ معیار پایه (پروفایل core)

| معیار | واحد نهایی | جهت (TOPSIS) |
|--------|------------|--------------|
| هزینه سخت‌افزار (CAPEX) | USD | کمینه |
| مصرف انرژی | mW | کمینه |
| بودجه لینک | dB | بیشینه |
| تأخیر | ms | کمینه |
| نرخ داده | Mbps | بیشینه |
| برد ارسال | km | بیشینه |
| حداکثر اندازه شبکه | تعداد گره | بیشینه |
| پهنای باند کانال | kHz | بیشینه |

## ستون‌های حذف‌شده

این ستون‌ها برای K-Means و TOPSIS حذف شدند چون **کیفی، توصیفی یا تکراری** هستند:

- OPEX سالانه (تقریباً همه صفر)
- پشتیبانی سلولی، مدولاسیون، نوع طیف، باند فرکانسی
- مکانیزم امنیت، نوع دوبلکس
- کاربرد اصلی، شرایط تست

## نرمال‌سازی

1. تبدیل واحدها به مقیاس یکسان (دلار، mW، km، Mbps، …)
2. `log1p` روی CAPEX، نرخ داده و اندازه شبکه (به‌خاطر دامنه وسیع)
3. `StandardScaler` از scikit-learn (مطابق پروپوزال)

## نکات مهم برای بررسی شما

قبل از K-Means این موارد را در فایل `raw` تأیید کنید:

| فناوری | موضوع | اقدام پیشنهادی |
|--------|--------|----------------|
| NB-IoT | برد اشتباه در منبع (`kilobytes/s`) | ۱۵ km جایگزین شد — منبع را چک کنید |
| LoRaWAN | CAPEX از PDF: `23,208,860` تومان | ≈ $387 — با قیمت بازار مقایسه کنید |
| LoRaWAN | تأخیر ۶۰۰۰۰ ms | احتمالاً داده Sigfox اشتباه وارد شده |
| 5G RedCap | نرخ داده `150000000` | به ۱۵۰ Mbps تبدیل شد (bps → Mbps) |
| Wi-Fi 6 | برد ۴۰ m | در منبع اصلاح شد |
| BLE / Thread | برد ۱۵۰ m | در منبع اصلاح شد (نه ۱۵۰ km) |

## ۱۳ فناوری vs ۶ فناوری شاخص

پروپوزال ۶ فناوری شاخص ذکر کرده؛ شما ۱۳ فناوری دارید. هر دو رویکرد معتبر است:

- **همه ۱۳** → خوشه‌بندی غنی‌تر
- **۶ شاخص** (مثلاً Wi-Fi 6, NB-IoT, LoRaWAN, BLE, Zigbee, LTE-M) → مطابق متن پروپوزال

برای محدود کردن، ردیف‌های غیرضروری را از `raw` CSV حذف کنید و اسکریپت را دوباره اجرا کنید.

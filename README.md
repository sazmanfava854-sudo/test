# IoT Network Recommendation System

سیستم پیشنهاددهنده شبکه IoT — پروژه پایان‌نامه کارشناسی ارشد

## معماری

| فاز | روش |
|-----|-----|
| 1. خوشه‌بندی | K-Means (سفارشی) |
| 2. وزن‌دهی | AHP چندکارشناسه |
| 3. وزن‌دهی تطبیقی | پرسشنامه + فرمول نمایی |
| 4. رتبه‌بندی | TOPSIS، VIKOR، COPRAS |

## پیش‌نیاز

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## اجرا

```bash
git clone https://github.com/sazmanfava854-sudo/test.git
cd test
dotnet run --project IoTRecommendation.Web
```

سپس مرورگر را باز کنید: `http://localhost:5000` (یا پورت نمایش‌داده‌شده در ترمینال)

## ساختار پروژه

```
IoTRecommendation.sln
├── IoTRecommendation.Core/          # الگوریتم‌ها و سرویس‌ها
├── IoTRecommendation.Infrastructure/ # خواندن JSON
└── IoTRecommendation.Web/           # رابط کاربری MVC
    └── Data/                        # Technologies, Questions, Experts, Settings
```

## جریان کار

1. **Home** → شروع تحلیل
2. **AHP** → محاسبه وزن‌های کارشناسی
3. **Clustering** → خوشه‌بندی K-Means
4. **Cluster Selection** → انتخاب خوشه
5. **Questionnaire** → پاسخ به ۱۳ سؤال
6. **Results** → رتبه‌بندی TOPSIS / VIKOR / COPRAS + مقایسه Spearman

## تنظیمات

فایل `IoTRecommendation.Web/Data/Settings.json`:

- `adaptiveWeightAlpha` — ضریب فرمول وزن تطبیقی
- `vikorV` — پارامتر v در VIKOR
- پارامترهای K-Means

## شاخه‌های توسعه

| شاخه | محتوا |
|------|-------|
| `main` | نسخه کامل (همه روش‌ها) |
| `cursor/iot-recommendation-system-d843` | پایه + TOPSIS |
| `cursor/vikor-evaluation-d843` | + VIKOR |
| `cursor/copras-evaluation-d843` | + COPRAS |

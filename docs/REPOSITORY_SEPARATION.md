# جداسازی مخزن‌های Git

در حال حاضر سه پروژهٔ مستقل دارید که **نباید در یک ZIP / یک مخزن قاطی شوند**:

## ۱. IoTRecommendation

- **محتوا:** `IoTRecommendation.sln`, `IoTRecommendation.Core`, `IoTRecommendation.Infrastructure`, `IoTRecommendation.Web`, `IoTRecommendation.Core.Tests`
- **مخزن پیشنهادی:** همین مخزن (شاخهٔ IoT / `main` پس از جداسازی)
- **اجرا:** `dotnet run --project IoTRecommendation.Web`

## ۲. HR Performance

- **محتوا:** `HRPerformance.sln`, `src/`, `frontend/`, `database/`, اسکریپت‌های `start.*` مخصوص HR
- **مخزن پیشنهادی:** مخزن جدا، مثلاً `HRPerformance` یا `hr-performance-system`
- **انتقال (یک‌بار):**

```bash
# روی نسخهٔ قدیمی monorepo که HR هنوز دارد
git subtree split -P src -b hr-src-only   # یا کل پوشه‌های HR را کپی کنید
# مخزن جدید بسازید در GitHub، سپس:
git remote add hr-origin git@github.com:ORG/HRPerformance.git
git push hr-origin main
```

ساده‌تر: از release قدیمی `HRPerformance-System-v1.0.1-final.zip` استفاده کنید و آن را به‌عنوان commit اول مخزن جدید push کنید.

## ۳. rayvarzresend

- **مخزن جدا** — در مخزن `test` / شاخهٔ IoT **وجود ندارد**.
- پروژه را در GitHub به‌صورت repo مستقل بسازید و فقط کد Rayvarz را آنجا نگه دارید.

## چرا ZIP شاخهٔ PR حجیم بود؟

وقتی HR، `releases/*.zip` و `wwwroot/lib` داخل **همان مخزن** بودند، دانلود شاخه ~۷ مگ (فشرده) می‌شد. پس از جداسازی، ZIP شاخهٔ IoT فقط سورس IoT + کتابخانه‌های وب است.

## قوانین نگهداری

- **Commit نکنید:** `bin/`, `obj/`, `IoTRecommendation-Project.zip` داخل Git (از Releases استفاده کنید).
- هر پروژه **یک README** و **یک solution** در ریشهٔ مخزن خودش.

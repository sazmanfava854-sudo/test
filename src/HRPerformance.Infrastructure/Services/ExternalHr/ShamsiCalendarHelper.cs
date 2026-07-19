using System.Globalization;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class ShamsiCalendarHelper
{
    private static readonly PersianCalendar Calendar = new();

    public static DateTime ToGregorianDate(int year, int month, int day)
    {
        if (year < 1300 || year > 1500)
        {
            if (year is >= 1990 and <= 2100)
            {
                throw new ArgumentOutOfRangeException(nameof(year),
                    $"سال {year} میلادی است. تاریخ شمسی وارد کنید — مثلاً 1404/04/10 نه 2025/07/01.");
            }

            throw new ArgumentOutOfRangeException(nameof(year),
                $"سال شمسی {year} نامعتبر است. بازه مجاز: 1300 تا 1500.");
        }
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "ماه شمسی نامعتبر است");

        var daysInMonth = Calendar.GetDaysInMonth(year, month);
        if (day < 1 || day > daysInMonth)
            throw new ArgumentOutOfRangeException(nameof(day), "روز شمسی نامعتبر است");

        var gregorian = Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        EnsureGregorianDate(gregorian, year, month, day);
        return gregorian;
    }

    /// <summary>
    /// Rejects dates that look like Shamsi values used as Gregorian (e.g. 1404-04-10).
    /// </summary>
    public static void EnsureGregorianDate(DateTime date, int shamsiYear, int shamsiMonth, int shamsiDay)
    {
        if (date.Year is >= 1300 and <= 1500)
        {
            throw new InvalidOperationException(
                $"تبدیل شمسی {shamsiYear}/{shamsiMonth:D2}/{shamsiDay:D2} به میلادی انجام نشده است " +
                $"(نتیجه: {date:yyyy-MM-dd}). لطفاً dotnet build را اجرا کنید و از نسخه 2.8.6-dev به بعد استفاده کنید.");
        }

        if (date.Year < 1990 || date.Year > 2100)
        {
            throw new InvalidOperationException(
                $"تبدیل شمسی {shamsiYear}/{shamsiMonth:D2}/{shamsiDay:D2} به میلادی نامعتبر است ({date:yyyy-MM-dd}).");
        }
    }

    public static int ToYearMonthKey(int year, int month) => year * 100 + month;
}

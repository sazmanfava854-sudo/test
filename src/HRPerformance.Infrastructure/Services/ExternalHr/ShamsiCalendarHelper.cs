using System.Globalization;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

/// <summary>
/// شمسی → میلادی (فقط تاریخ، بدون ساعت)
/// </summary>
public static class ShamsiCalendarHelper
{
    private static readonly PersianCalendar Calendar = new();

    public static DateTime ToGregorianDateOnly(int shamsiYear, int shamsiMonth, int shamsiDay)
    {
        if (shamsiYear is < 1300 or > 1500)
        {
            if (shamsiYear is >= 1990 and <= 2100)
                throw new ArgumentOutOfRangeException(nameof(shamsiYear),
                    $"سال {shamsiYear} میلادی است. تاریخ شمسی بدهید — مثلاً 1404/04/10");

            throw new ArgumentOutOfRangeException(nameof(shamsiYear),
                $"سال شمسی {shamsiYear} نامعتبر است (1300–1500).");
        }

        if (shamsiMonth is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(shamsiMonth), "ماه شمسی 1–12");

        var maxDay = Calendar.GetDaysInMonth(shamsiYear, shamsiMonth);
        if (shamsiDay < 1 || shamsiDay > maxDay)
            throw new ArgumentOutOfRangeException(nameof(shamsiDay), $"روز شمسی 1–{maxDay}");

        // PersianCalendar → DateTime میلادی
        var gregorian = Calendar.ToDateTime(shamsiYear, shamsiMonth, shamsiDay, 0, 0, 0, 0).Date;

        if (!IsGregorianDate(gregorian))
        {
            throw new InvalidOperationException(
                $"تبدیل {shamsiYear}/{shamsiMonth:D2}/{shamsiDay:D2} → {gregorian:yyyy-MM-dd} نامعتبر است. " +
                "dotnet build --no-incremental src\\HRPerformance.API\\HRPerformance.API.csproj");
        }

        return gregorian;
    }

    public static bool IsGregorianDate(DateTime date) =>
        date.Year is >= 1990 and <= 2100;

    /// <summary>1404/04/10 → 14040410</summary>
    public static int ToDateKey(int year, int month, int day) =>
        year * 10000 + month * 100 + day;

    public static string FormatDate(int year, int month, int day) =>
        $"{year}/{month:D2}/{day:D2}";

    public static int ToYearMonthKey(int year, int month) => year * 100 + month;
}

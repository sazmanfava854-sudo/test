using System.Globalization;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class ShamsiCalendarHelper
{
    private static readonly PersianCalendar Calendar = new();

    public static DateTime ToGregorianDate(int year, int month, int day)
    {
        if (year < 1300 || year > 1500)
            throw new ArgumentOutOfRangeException(nameof(year), "سال شمسی نامعتبر است");
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "ماه شمسی نامعتبر است");

        var daysInMonth = Calendar.GetDaysInMonth(year, month);
        if (day < 1 || day > daysInMonth)
            throw new ArgumentOutOfRangeException(nameof(day), "روز شمسی نامعتبر است");

        return Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
    }

    public static int ToYearMonthKey(int year, int month) => year * 100 + month;
}

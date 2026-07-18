using System.Globalization;

namespace HRPerformance.Services.ExternalHr;

public static class ShamsiDateHelper
{
    private static readonly PersianCalendar Calendar = new();

    public static (int Year, int Month) GetCurrentShamsi()
    {
        var now = DateTime.Now;
        return (Calendar.GetYear(now), Calendar.GetMonth(now));
    }

    public static (DateTime Start, DateTime EndExclusive) GetGregorianMonthRange(int shamsiYear, int shamsiMonth)
    {
        if (shamsiMonth is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(shamsiMonth));

        var start = Calendar.ToDateTime(shamsiYear, shamsiMonth, 1, 0, 0, 0, 0);
        var (nextYear, nextMonth) = shamsiMonth == 12
            ? (shamsiYear + 1, 1)
            : (shamsiYear, shamsiMonth + 1);
        var endExclusive = Calendar.ToDateTime(nextYear, nextMonth, 1, 0, 0, 0, 0);
        return (start, endExclusive);
    }

    public static (int Year, int Month) AddMonths(int shamsiYear, int shamsiMonth, int delta)
    {
        var date = Calendar.ToDateTime(shamsiYear, shamsiMonth, 1, 0, 0, 0, 0);
        date = Calendar.AddMonths(date, delta);
        return (Calendar.GetYear(date), Calendar.GetMonth(date));
    }

    public static int Compare(int yearA, int monthA, int yearB, int monthB)
    {
        if (yearA != yearB) return yearA.CompareTo(yearB);
        return monthA.CompareTo(monthB);
    }
}

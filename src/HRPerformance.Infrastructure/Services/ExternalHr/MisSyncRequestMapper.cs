using HRPerformance.Domain.Models;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class MisSyncRequestMapper
{
    public static MisSyncRange ToSyncRange(MisSyncDateRangeRequest request)
    {
        var fromGregorian = ShamsiCalendarHelper.ToGregorianDate(
            request.ShamsiFromYear, request.ShamsiFromMonth, request.ShamsiFromDay);
        var toGregorian = ShamsiCalendarHelper.ToGregorianDate(
            request.ShamsiToYear, request.ShamsiToMonth, request.ShamsiToDay);

        if (toGregorian < fromGregorian)
            throw new ArgumentException("تاریخ پایان شمسی باید بعد از تاریخ شروع باشد");

        return new MisSyncRange
        {
            SyncFrom = fromGregorian.Date,
            SyncToExclusive = toGregorian.Date.AddDays(1),
            ShamsiFromYm = ShamsiCalendarHelper.ToYearMonthKey(request.ShamsiFromYear, request.ShamsiFromMonth),
            ShamsiToYm = ShamsiCalendarHelper.ToYearMonthKey(request.ShamsiToYear, request.ShamsiToMonth),
            Description =
                $"شمسی {request.ShamsiFromYear}/{request.ShamsiFromMonth:D2}/{request.ShamsiFromDay:D2} تا " +
                $"{request.ShamsiToYear}/{request.ShamsiToMonth:D2}/{request.ShamsiToDay:D2} " +
                $"(میلادی {fromGregorian:yyyy-MM-dd} تا {toGregorian:yyyy-MM-dd})"
        };
    }

    public static (DateTime From, DateTime To) ToGregorianRange(MisSyncDateRangeRequest request)
    {
        var range = ToSyncRange(request);
        return (range.SyncFrom, range.SyncToExclusive.AddDays(-1));
    }
}

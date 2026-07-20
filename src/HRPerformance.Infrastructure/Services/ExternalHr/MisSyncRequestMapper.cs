using HRPerformance.Domain.Models;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class MisSyncRequestMapper
{
    public static MisSyncRange ToSyncRange(MisSyncDateRangeRequest request)
    {
        var fromKey = ShamsiCalendarHelper.ToDateKey(
            request.ShamsiFromYear, request.ShamsiFromMonth, request.ShamsiFromDay);
        var toKey = ShamsiCalendarHelper.ToDateKey(
            request.ShamsiToYear, request.ShamsiToMonth, request.ShamsiToDay);

        if (toKey < fromKey)
            throw new ArgumentException("تاریخ پایان شمسی باید بعد از تاریخ شروع باشد");

        var fromText = ShamsiCalendarHelper.FormatDate(
            request.ShamsiFromYear, request.ShamsiFromMonth, request.ShamsiFromDay);
        var toText = ShamsiCalendarHelper.FormatDate(
            request.ShamsiToYear, request.ShamsiToMonth, request.ShamsiToDay);

        return new MisSyncRange
        {
            ShamsiFromKey = fromKey,
            ShamsiToKey = toKey,
            ShamsiFromText = fromText,
            ShamsiToText = toText,
            ShamsiFromYm = ShamsiCalendarHelper.ToYearMonthKey(request.ShamsiFromYear, request.ShamsiFromMonth),
            ShamsiToYm = ShamsiCalendarHelper.ToYearMonthKey(request.ShamsiToYear, request.ShamsiToMonth),
            Description = $"شمسی {fromText} تا {toText}"
        };
    }

    /// <summary>فقط برای نمایش رکوردهای ذخیره‌شده در DB محلی (میلادی)</summary>
    public static (DateTime From, DateTime To) ToGregorianRange(MisSyncDateRangeRequest request)
    {
        var from = ShamsiCalendarHelper.ToGregorianDateOnly(
            request.ShamsiFromYear, request.ShamsiFromMonth, request.ShamsiFromDay);
        var to = ShamsiCalendarHelper.ToGregorianDateOnly(
            request.ShamsiToYear, request.ShamsiToMonth, request.ShamsiToDay);
        return (from, to);
    }
}

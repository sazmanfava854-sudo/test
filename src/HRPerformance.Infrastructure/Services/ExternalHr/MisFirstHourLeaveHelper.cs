namespace HRPerformance.Infrastructure.Services.ExternalHr;

/// <summary>
/// مرخصی ساعتی ۰۷:۰۰–۰۸:۰۰ = مرخصی اول وقت → ثبت به‌عنوان تاخیر
/// </summary>
public static class MisFirstHourLeaveHelper
{
    public static readonly TimeSpan WindowStart = new(7, 0, 0);
    public static readonly TimeSpan WindowEnd = new(8, 0, 0);

    /// <summary>
    /// مرخصی با شروع بین ۷:۰۰ تا ۸:۰۰ (یا FirstTimeType=1 در MIS)
    /// </summary>
    public static bool IsFirstHourLeave(TimeSpan? startTime, int firstTimeType)
    {
        if (firstTimeType == 1)
            return true;

        if (!startTime.HasValue)
            return false;

        var t = startTime.Value;
        if (t.Days > 0)
            t = t.Subtract(TimeSpan.FromDays(t.Days));

        return t >= WindowStart && t < WindowEnd;
    }

    /// <summary>
    /// مدت مرخصی اول وقت = دقیقه تاخیر (مثال: ۰۷:۰۰–۰۷:۳۱ → ۳۲ دقیقه)
    /// </summary>
    public static int ComputeDelayMinutes(TimeSpan? startTime, int leaveDurationMinutes, int firstTimeType)
    {
        if (!IsFirstHourLeave(startTime, firstTimeType))
            return 0;

        return Math.Max(leaveDurationMinutes, 0);
    }

    public static string BuildLeaveTypeLabel(int firstTimeType, int delayMinutes) =>
        delayMinutes > 0
            ? $"مرخصی اول وقت (تاخیر {delayMinutes} دقیقه)"
            : $"مرخصی ساعتی - نوع {firstTimeType}";
}

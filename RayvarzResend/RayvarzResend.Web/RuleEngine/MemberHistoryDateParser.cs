using System.Globalization;

namespace RayvarzResend.Web.RuleEngine;

/// <summary>MemberHistory.ModifyDate/ModifyTime اغلب شمسی varchar است — نه datetime SQL.</summary>
public static class MemberHistoryDateParser
{
    public static DateTime CombineModifyDateTime(object? modifyDate, object? modifyTime)
    {
        if (TryParseModifyDateTime(modifyDate, modifyTime, out var dt))
            return dt;

        return DateTime.UtcNow;
    }

    public static bool TryParseModifyDateTime(object? modifyDate, object? modifyTime, out DateTime result)
    {
        result = default;
        if (modifyDate == null || modifyDate is DBNull)
            return false;

        var time = ParseTime(modifyTime);

        if (modifyDate is DateTime dateTime)
        {
            if (dateTime.Year is >= 1300 and <= 1500)
            {
                result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                    time.Hours, time.Minutes, time.Seconds, DateTimeKind.Unspecified);
                return true;
            }

            result = dateTime.Date.Add(time);
            return true;
        }

        var dateStr = modifyDate.ToString()?.Trim();
        if (string.IsNullOrEmpty(dateStr))
            return false;

        var digits = new string(dateStr.Where(char.IsDigit).ToArray());
        if (digits.Length >= 8
            && int.TryParse(digits[..4], out var y)
            && int.TryParse(digits.Substring(4, 2), out var m)
            && int.TryParse(digits.Substring(6, 2), out var d)
            && y is >= 1300 and <= 1500)
        {
            try
            {
                var pc = new PersianCalendar();
                result = pc.ToDateTime(y, m, d, time.Hours, time.Minutes, time.Seconds, 0);
                return true;
            }
            catch
            {
                // fall through
            }
        }

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            || DateTime.TryParse(dateStr, new CultureInfo("fa-IR"), DateTimeStyles.None, out parsed))
        {
            result = parsed.Date.Add(time);
            return true;
        }

        return false;
    }

    private static TimeSpan ParseTime(object? modifyTime)
    {
        if (modifyTime == null || modifyTime is DBNull)
            return TimeSpan.Zero;

        return modifyTime switch
        {
            TimeSpan ts => ts,
            DateTime dt => dt.TimeOfDay,
            _ when TimeSpan.TryParse(modifyTime.ToString(), out var parsed) => parsed,
            _ => TimeSpan.Zero
        };
    }
}

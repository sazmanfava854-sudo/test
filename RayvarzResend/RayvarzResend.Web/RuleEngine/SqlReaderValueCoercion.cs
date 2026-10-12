using System.Globalization;

namespace RayvarzResend.Web.RuleEngine;

/// <summary>خواندن مقادیر تاریخ/زمان از SqlDataReader وقتی نوع ستون در سرور legacy متفاوت است (مثلاً nvarchar).</summary>
internal static class SqlReaderValueCoercion
{
    public static DateTime? ToNullableDateTime(object? value)
    {
        if (value is null or DBNull)
            return null;

        switch (value)
        {
            case DateTime dt:
                return dt;
            case DateTimeOffset dto:
                return dto.DateTime;
            case string s when !string.IsNullOrWhiteSpace(s):
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var inv))
                    return inv;
                if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var cur))
                    return cur;
                return null;
            default:
                try
                {
                    return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException)
                {
                    return null;
                }
                catch (FormatException)
                {
                    return null;
                }
        }
    }

    public static TimeSpan? ToNullableTimeOfDay(object? value)
    {
        if (value is null or DBNull)
            return null;

        switch (value)
        {
            case TimeSpan ts:
                return ts;
            case DateTime dt:
                return dt.TimeOfDay;
            case string s when !string.IsNullOrWhiteSpace(s):
                if (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out var t))
                    return t;
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var asDt))
                    return asDt.TimeOfDay;
                return null;
            default:
                try
                {
                    return Convert.ToDateTime(value, CultureInfo.InvariantCulture).TimeOfDay;
                }
                catch (InvalidCastException)
                {
                    return null;
                }
                catch (FormatException)
                {
                    return null;
                }
        }
    }

    public static DateTime? CombineDateAndTime(DateTime? date, TimeSpan? timeOfDay)
    {
        if (date is null)
            return null;
        if (timeOfDay is null)
            return date;
        return date.Value.Date.Add(timeOfDay.Value);
    }
}

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
                return dt.Year < 1700 ? FromShamsiParts(dt.Year, dt.Month, dt.Day) : dt;
            case DateTimeOffset dto:
                return dto.DateTime;
            case string s when !string.IsNullOrWhiteSpace(s):
                if (TryParseShamsi(s, out var shamsi))
                    return shamsi;
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var inv))
                    return inv.Year < 1700 ? FromShamsiParts(inv.Year, inv.Month, inv.Day) : inv;
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

    /// <summary>"1403/07/20" یا "14030720" (سال &lt; 1700 = شمسی).</summary>
    public static bool TryParseShamsi(string s, out DateTime result)
    {
        result = default;
        var digits = ToLatinDigits(s.Trim());
        var datePart = digits.Split(' ', 'T')[0];

        int y, m, d;
        var parts = datePart.Split('/', '-', '.');
        if (parts.Length == 3
            && int.TryParse(parts[0], out y)
            && int.TryParse(parts[1], out m)
            && int.TryParse(parts[2], out d))
        {
        }
        else if (datePart.Length == 8 && int.TryParse(datePart, out var packed))
        {
            y = packed / 10000;
            m = packed / 100 % 100;
            d = packed % 100;
        }
        else
        {
            return false;
        }

        if (y is < 1200 or >= 1700)
            return false;

        var converted = FromShamsiParts(y, m, d);
        if (converted is null)
            return false;
        result = converted.Value;
        return true;
    }

    /// <summary>datetime در SQL Server قبل از 1753 را نمی‌پذیرد.</summary>
    public static object ToSqlDateTimeParameter(DateTime? value)
    {
        if (value is null || value.Value.Year < 1753)
            return DBNull.Value;
        return value.Value;
    }

    private static DateTime? FromShamsiParts(int year, int month, int day)
    {
        try
        {
            return new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string ToLatinDigits(string s)
    {
        var chars = s.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c is >= '\u06F0' and <= '\u06F9')
                chars[i] = (char)('0' + (c - '\u06F0'));
            else if (c is >= '\u0660' and <= '\u0669')
                chars[i] = (char)('0' + (c - '\u0660'));
        }
        return new string(chars);
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

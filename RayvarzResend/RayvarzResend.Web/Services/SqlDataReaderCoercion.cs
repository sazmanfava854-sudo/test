using System.Globalization;
using Microsoft.Data.SqlClient;

namespace RayvarzResend.Web.Services;

/// <summary>خواندن امن از SqlDataReader وقتی نوع ستون legacy (مثلاً nvarchar) با CLR متفاوت است.</summary>
public static class SqlDataReaderCoercion
{
    public static int ReadInt32(SqlDataReader reader, string column, int defaultValue = 0)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord))
            return defaultValue;
        return ToInt32(reader.GetValue(ord), defaultValue);
    }

    public static int ToInt32(object? value, int defaultValue = 0)
    {
        if (value is null or DBNull)
            return defaultValue;

        switch (value)
        {
            case int i:
                return i;
            case long l:
                return (int)l;
            case short s:
                return s;
            case byte b:
                return b;
            case decimal d:
                return (int)d;
            case double dbl:
                return (int)dbl;
            case float f:
                return (int)f;
            case string str:
                return int.TryParse(str.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : defaultValue;
            default:
                try
                {
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
                {
                    return defaultValue;
                }
        }
    }

    public static decimal ReadDecimal(SqlDataReader reader, string column, decimal defaultValue = 0)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord))
            return defaultValue;
        return ToDecimal(reader.GetValue(ord), defaultValue);
    }

    public static decimal ToDecimal(object? value, decimal defaultValue = 0)
    {
        if (value is null or DBNull)
            return defaultValue;

        switch (value)
        {
            case decimal d:
                return d;
            case int i:
                return i;
            case long l:
                return l;
            case double dbl:
                return (decimal)dbl;
            case float f:
                return (decimal)f;
            case string str:
                return decimal.TryParse(str.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : defaultValue;
            default:
                try
                {
                    return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
                {
                    return defaultValue;
                }
        }
    }

    public static string ReadString(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord))
            return "";
        return reader.GetValue(ord)?.ToString()?.Trim() ?? "";
    }
}

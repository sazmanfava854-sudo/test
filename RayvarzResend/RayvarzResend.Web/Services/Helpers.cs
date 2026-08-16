namespace RayvarzResend.Web.Services;

public static class IncomeExcludedCodes
{
    public static readonly HashSet<int> Codes = new()
    {
        0, 100036, 100041, 100042, 100043, 100047, 100048, 100049, 100050, 100052,
        100028, 100016, 100009, 100002, 1091, 1101, 100061, 100055, 100057, 100060,
        100200, 100075, 100067, 100068, 100087, 999999, 120, 100006, 100072, 100032,
        100080, 100101, 100045, 100102, 100103, 100104, 100105, 100097, 100098,
        100099, 100109, 100081, 100114, 100082, 100053, 100029, 1301, 100202
    };
}

public static class DateHelper
{
    private const int MinShamsiYear = 1300;
    private const int MaxShamsiYear = 1500;

    public static string ToRayvarzDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var trimmed = NumericHelper.NormalizeDigits(input.Trim());
        if (trimmed.Contains('/') || trimmed.Contains('-'))
        {
            var parts = trimmed.Split(['/', '-'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3
                && int.TryParse(parts[0], out var y)
                && int.TryParse(parts[1], out var m)
                && int.TryParse(parts[2], out var d))
            {
                return FormatShamsiYyyyMmDd(y, m, d);
            }
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return "";

        if (digits.Length >= 8)
        {
            var candidate = digits[..8];
            return IsValidShamsiYyyyMmDd(candidate) ? candidate : "";
        }

        // 7 رقم بدون صفر اول سال — مثل 4050505 → 14050505 (نه PadLeft → 04050505)
        if (digits.Length == 7 && digits[0] is '3' or '4')
        {
            var withLeadingOne = "1" + digits;
            if (IsValidShamsiYyyyMmDd(withLeadingOne))
                return withLeadingOne;
        }

        return "";
    }

    public static string CurrentShamsiRayvarzDate()
    {
        var pc = new System.Globalization.PersianCalendar();
        var now = DateTime.Now;
        return $"{pc.GetYear(now):0000}{pc.GetMonth(now):00}{pc.GetDayOfMonth(now):00}";
    }

    /// <summary>تاریخ شمسی با اسلش — مطابق ستون‌های Income_Fiche (مثلاً 1405/03/23).</summary>
    public static string CurrentShamsiSlashDate()
    {
        var d = CurrentShamsiRayvarzDate();
        return d.Length >= 8 ? $"{d[..4]}/{d.Substring(4, 2)}/{d.Substring(6, 2)}" : d;
    }

    public static int CurrentShamsiYear() => ExtractShamsiYear(CurrentShamsiRayvarzDate());

    /// <summary>سال شمسی از رشته yyyyMMdd (برای ستون yr در incmdocsys).</summary>
    public static int ExtractShamsiYear(string rayvarzYyyyMmDd)
    {
        var d = ToRayvarzDate(rayvarzYyyyMmDd);
        return d.Length >= 4 && int.TryParse(d[..4], out var y) ? y : 0;
    }

    /// <summary>تاریخ دیتابیس: اگر سال ۱۳xx–۱۴xx باشد همان تقویم شمسی ذخیره‌شده؛ وگرنه تبدیل میلادی→شمسی.</summary>
    public static string FromDatabaseDateValue(object value)
    {
        if (value is DateTime dt)
        {
            if (dt.Year is >= 1300 and <= 1500)
                return dt.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            var pc = new System.Globalization.PersianCalendar();
            return $"{pc.GetYear(dt):0000}{pc.GetMonth(dt):00}{pc.GetDayOfMonth(dt):00}";
        }

        return ToRayvarzDate(value.ToString() ?? "");
    }

    public static string ToShamsiSlashDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        if (input.Contains('/')) return input.Trim();
        var d = ToRayvarzDate(input);
        return d.Length >= 8 ? $"{d[..4]}/{d.Substring(4, 2)}/{d.Substring(6, 2)}" : input.Trim();
    }

    /// <summary>تبدیل تاریخ شمسی به DateTime با اجزای شمسی (مطابق ذخیره datetime در Sara).</summary>
    public static DateTime? ToSqlDateTimeFromRayvarz(string? input)
    {
        var d = ToRayvarzDate(input ?? "");
        if (d.Length < 8) return null;
        if (!int.TryParse(d[..4], out var y)) return null;
        if (!int.TryParse(d.Substring(4, 2), out var m)) return null;
        if (!int.TryParse(d.Substring(6, 2), out var day)) return null;
        try
        {
            return new DateTime(y, m, day);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>پایان بازه — روز بعد برای مقایسه {@code &lt;}.</summary>
    public static DateTime? ToSqlDateTimeEndExclusiveFromRayvarz(string? input)
    {
        var dt = ToSqlDateTimeFromRayvarz(input);
        return dt?.AddDays(1);
    }

    private static string FormatShamsiYyyyMmDd(int year, int month, int day)
    {
        if (year is < MinShamsiYear or > MaxShamsiYear) return "";
        if (month is < 1 or > 12) return "";
        if (day is < 1 or > 31) return "";
        return $"{year:0000}{month:00}{day:00}";
    }

    private static bool IsValidShamsiYyyyMmDd(string yyyyMmDd)
    {
        if (yyyyMmDd.Length != 8) return false;
        if (!int.TryParse(yyyyMmDd[..4], out var year)) return false;
        if (!int.TryParse(yyyyMmDd.Substring(4, 2), out var month)) return false;
        if (!int.TryParse(yyyyMmDd.Substring(6, 2), out var day)) return false;
        return FormatShamsiYyyyMmDd(year, month, day) == yyyyMmDd;
    }
}

/// <summary>سال‌های محتمل برای جستجو در incmdocsys — SOAP ممکن است سال متفاوت از PaymentDate ذخیره کند.</summary>
public static class RayvarzYearResolver
{
    public static IReadOnlyList<int> CollectCandidates(params string?[] dateSources)
    {
        var years = new HashSet<int>();
        foreach (var source in dateSources)
        {
            var year = DateHelper.ExtractShamsiYear(source ?? "");
            if (year > 0)
                years.Add(year);
        }

        var current = DateHelper.CurrentShamsiYear();
        if (current > 0)
            years.Add(current);

        return years.OrderByDescending(y => y).ToList();
    }
}

/// <summary>اعداد legacy Sara — ممکن است فارسی/عربی یا nvarchar باشند.</summary>
public static class NumericHelper
{
    public static long? TryParseLegacyLong(object? value)
    {
        if (value is null or DBNull) return null;
        if (value is long l) return l;
        if (value is int i) return i;
        if (value is decimal d) return (long)d;
        if (value is double dbl) return (long)dbl;

        var s = value.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;

        s = NormalizeDigits(s);
        if (s.Contains('.') || s.Contains(','))
        {
            var dec = s.Replace(',', '.');
            if (decimal.TryParse(dec, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsedDec))
                return (long)parsedDec;
            return null;
        }

        s = new string(s.Where(c => char.IsDigit(c) || c == '-').ToArray());
        if (string.IsNullOrWhiteSpace(s)) return null;

        return long.TryParse(s, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var parsedLong)
            ? parsedLong
            : null;
    }

    public static string NormalizeDigits(string input)
    {
        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= '\u06F0' and <= '\u06F9')
                chars[i] = (char)('0' + (chars[i] - '\u06F0'));
            else if (chars[i] is >= '\u0660' and <= '\u0669')
                chars[i] = (char)('0' + (chars[i] - '\u0660'));
        }

        return new string(chars);
    }
}

public static class FundResolver
{
    public static int Resolve(IConfiguration config, int branch, string bankCode)
    {
        // nosazo.vb — bank-aware؛ FundMap فقط برای شعب ناشناخته
        if (branch is >= 201 and <= 212 or 218)
            return DutyDistrictBranchResolver.ResolveFund(branch, bankCode);

        var key = branch.ToString();
        if (config.GetSection("FundMap").Exists() && config[$"FundMap:{key}"] != null)
            return config.GetValue<int>($"FundMap:{key}");

        return bankCode == "1" ? 1200 : 1300;
    }
}

/// <summary>هم‌راستا با appsettings — در شبکه داخلی MSB اغلب http است نه https.</summary>
public static class RayvarzUrlNormalizer
{
    public static string Normalize(IConfiguration config, string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        var u = url.Trim();
        if (config.GetValue("Rayvarz:UseHttp", true))
            u = u.Replace("https://", "http://", StringComparison.OrdinalIgnoreCase);
        return u;
    }
}

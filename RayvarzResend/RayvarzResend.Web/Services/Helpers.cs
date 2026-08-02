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
    public static string ToRayvarzDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length >= 8) return digits[..8];
        return digits.PadLeft(8, '0');
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

    public static string ToShamsiSlashDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        if (input.Contains('/')) return input.Trim();
        var d = ToRayvarzDate(input);
        return d.Length >= 8 ? $"{d[..4]}/{d.Substring(4, 2)}/{d.Substring(6, 2)}" : input.Trim();
    }

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
}

public static class FundResolver
{
    public static int Resolve(IConfiguration config, int branch, string bankCode)
    {
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

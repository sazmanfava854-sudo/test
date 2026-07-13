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
        if (string.IsNullOrWhiteSpace(input)) return DateTime.Now.ToString("yyyyMMdd");
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length >= 8) return digits[..8];
        return digits.PadLeft(8, '0');
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

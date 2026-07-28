namespace RayvarzResend.Web.Services;

/// <summary>تعیین DistrickBranch و Fund — nosazo.vb خط ۲۲–۱۹۳ و ۴۵۷–۵۱۸.</summary>
public static class DutyDistrictBranchResolver
{
    private sealed record DistrictRule(string? Ptmp, string? BillPrefix, int LegacyBranch, int ModernBranch, string CutoffShamsi);

    private static readonly DistrictRule[] Rules =
    {
        new("511", "01", 1, 201, "1396/08/26"),
        new("512", "02", 2, 202, "1396/10/15"),
        new("513", "03", 3, 203, "1397/03/04"),
        new("514", "04", 4, 204, "1397/03/13"),
        new("515", "05", 5, 205, "1397/03/22"),
        new("516", "06", 6, 206, "1397/03/27"),
        new("517", "07", 7, 207, "1397/03/31"),
        new("518", "08", 8, 208, "1397/04/07"),
        new("519", "09", 9, 209, "1397/04/28"),
        new("520", "10", 10, 210, "1397/05/03"),
        new("521", "11", 11, 211, "1397/05/07"),
        new("522", "12", 12, 212, "1397/05/15"),
        new("523", "80", 80, 218, "1397/05/20"),
    };

    public static int ResolveBranch(string billId, string paymentId, string? shamsiToday = null)
    {
        if (string.IsNullOrWhiteSpace(billId))
            return 0;

        shamsiToday ??= CurrentShamsiSlashDate();

        if (billId.Length >= 11)
        {
            var ptmp = billId.Substring(8, 3);
            if (ptmp == "051" && paymentId.Length >= 11)
            {
                var two = paymentId.Substring(9, 2);
                if (int.TryParse(two, out var brFromPay))
                {
                    if (brFromPay > 12 && brFromPay != 80 && paymentId.Length > 10
                        && int.TryParse(paymentId.Substring(10, 1), out var brDigit))
                        return brDigit;
                    return brFromPay;
                }
            }

            var fromPtmp = MapRule(r => r.Ptmp == ptmp, shamsiToday);
            if (fromPtmp != 0)
                return fromPtmp;
        }

        if (billId.Length >= 2)
        {
            var prefix = billId.Substring(0, 2);
            return MapRule(r => r.BillPrefix == prefix, shamsiToday);
        }

        return 0;
    }

    private static int MapRule(Func<DistrictRule, bool> predicate, string shamsiToday)
    {
        var rule = Rules.FirstOrDefault(r => predicate(r));
        if (rule == null)
            return 0;

        return ShamsiCompare(shamsiToday, rule.CutoffShamsi) > 0
            ? rule.ModernBranch
            : rule.LegacyBranch;
    }

    private static string CurrentShamsiSlashDate()
    {
        var pc = new System.Globalization.PersianCalendar();
        var now = DateTime.Now;
        return $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";
    }

  /// <summary>مقایسه تاریخ شمسی با فرمت yyyy/MM/dd یا yyyyMMdd.</summary>
    private static int ShamsiCompare(string a, string b)
    {
        static string Norm(string s) => new string(s.Where(char.IsDigit).ToArray()).PadLeft(8, '0')[..8];
        return string.CompareOrdinal(Norm(a), Norm(b));
    }

    /// <summary>Fund از RefFund — nosazo.vb ۴۵۷–۵۱۸ (BankCode=1 شاخه جدا).</summary>
    public static int ResolveFund(int districtBranch, string bankCode)
    {
        var bankIsOne = bankCode == "1";
        return districtBranch switch
        {
            201 => bankIsOne ? 200201009 : 200201012,
            202 => bankIsOne ? 200202007 : 200202012,
            203 => bankIsOne ? 200203009 : 200203013,
            204 => bankIsOne ? 200204005 : 200204017,
            205 => bankIsOne ? 200205004 : 200205008,
            206 => bankIsOne ? 256222008 : 200206006,
            207 => bankIsOne ? 200207006 : 200207009,
            208 => bankIsOne ? 200208007 : 200208010,
            209 => bankIsOne ? 200209004 : 200209008,
            210 => bankIsOne ? 200210005 : 200210020,
            211 => bankIsOne ? 200211004 : 200211007,
            212 => bankIsOne ? 200212004 : 212210016,
            218 => bankIsOne ? 1200 : 200218011,
            _ => bankIsOne ? 1200 : 1300
        };
    }
}

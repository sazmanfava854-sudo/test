namespace RayvarzResend.Web.Services;

/// <summary>تعیین DistrickBranch از BillID/PaymentID — nosazo.vb خط ۲۲–۱۹۳ (همه cutoffهای تاریخ گذشته‌اند → شعبه ۲۰۱–۲۱۲ و ۲۱۸).</summary>
public static class DutyDistrictBranchResolver
{
    public static int ResolveBranch(string billId, string paymentId)
    {
        if (string.IsNullOrWhiteSpace(billId))
            return 0;

        if (billId.Length >= 11)
        {
            var ptmp = billId.Substring(8, 3);
            if (ptmp == "051" && paymentId.Length >= 11)
            {
                if (int.TryParse(paymentId.Substring(9, 2), out var brFromPay))
                {
                    if (brFromPay > 12 && brFromPay != 80 && paymentId.Length > 10
                        && int.TryParse(paymentId.Substring(10, 1), out var brDigit))
                        return brDigit;
                    return brFromPay;
                }
            }

            var fromPtmp = MapPtmpDistrict(ptmp);
            if (fromPtmp != 0)
                return fromPtmp;
        }

        if (billId.Length >= 2)
        {
            var prefix = billId.Substring(0, 2);
            return MapBillPrefix(prefix);
        }

        return 0;
    }

    private static int MapPtmpDistrict(string ptmp) => ptmp switch
    {
        "511" => 201,
        "512" => 202,
        "513" => 203,
        "514" => 204,
        "515" => 205,
        "516" => 206,
        "517" => 207,
        "518" => 208,
        "519" => 209,
        "520" => 210,
        "521" => 211,
        "522" => 212,
        "523" => 218,
        _ => 0
    };

    private static int MapBillPrefix(string prefix) => prefix switch
    {
        "01" => 201,
        "02" => 202,
        "03" => 203,
        "04" => 204,
        "05" => 205,
        "06" => 206,
        "07" => 207,
        "08" => 208,
        "09" => 209,
        "10" => 210,
        "11" => 211,
        "12" => 212,
        "80" => 218,
        _ => 0
    };

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
            218 => bankIsOne ? 1300 : 200218011,
            _ => bankIsOne ? 1200 : 1300
        };
    }
}

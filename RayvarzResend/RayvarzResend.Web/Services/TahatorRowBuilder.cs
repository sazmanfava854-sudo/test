using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ساخت ردیف SOAP تهاتر — منبع: تابع <c>Tahator1</c> در XmlBody Member NidMember=1388
/// (DisplayName: «تهاتر تک مبلغی»)، نه استنتاج از نمونه فیش.
/// شرط ورود مسیر: <c>CI_IncomeAccountGroup=157</c>.
/// توجه: تابع <c>Tahator</c> («درآمدی تهاتر») منطق Center متفاوتی دارد و اینجا پیاده نشده.
/// </summary>
public static class TahatorRowBuilder
{
    public const int IncomeAccountGroupTahator = 157;
    public const int IncmNoBank4 = 200098;
    public const int IncmNoOther = 200099;
    public const long Center3Default = 700100001;
    public const long Center3CheckNo5 = 700100002;

    /// <summary>
    /// پارامتر branch در SaveDocument برای اسناد تهاتر گروه ۱۵۷ در نمونه‌های واقعی رایورز ثابت ۱۰۲ است
    /// (نه DistrickBranch ۲۰۱–۲۱۲ که فقط برای Fund استفاده می‌شود).
    /// </summary>
    public const int DefaultRayvarzBranch = 102;

    /// <summary>Tahator1: PhasType = 2 → ptDraft</summary>
    public const string PhasTypCode = "2";

    /// <summary>Tahator1: vchrtyp = 1 → pfPay</summary>
    public const string VchrTypCode = "1";

    /// <summary>نمونه اصلی: ActTyp = 1</summary>
    public const string ActTypCode = "1";

    public static bool IsTahatorFiche(FicheHeaderDto fiche) =>
        fiche.Category == FicheCategory.Income
        && (fiche.IncomeAccountGroup == IncomeAccountGroupTahator
            || fiche.DocTyp is 14 or 15);

    /// <summary>
    /// Fund تهاتر از Tahator1 (RefFund): 201→51 … 209→59 … 218→63 — نه FundMap نوسازی 200xxx.
    /// </summary>
    public static int ResolveTahatorFund(int districtBranch) =>
        districtBranch switch
        {
            201 => 51,
            202 => 52,
            203 => 53,
            204 => 54,
            205 => 55,
            206 => 56,
            207 => 57,
            208 => 58,
            209 => 59,
            210 => 60,
            211 => 61,
            212 => 62,
            218 => 63,
            _ => 0
        };

    /// <summary>
    /// از اولین بخش BnkAcntNo (کد نوسازی GetNosaziNickName) منطقه را به 201–212 نگاشت می‌کند.
    /// </summary>
    public static int ResolveDistrictBranchFromNosaziCode(string? bnkAcntNo)
    {
        if (string.IsNullOrWhiteSpace(bnkAcntNo))
            return 0;
        var first = bnkAcntNo.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        if (!int.TryParse(first, out var district) || district <= 0)
            return 0;
        if (district is >= 201 and <= 212 or 218)
            return district;
        if (district == 80)
            return 218;
        if (district is >= 1 and <= 12)
            return 200 + district;
        return 0;
    }

    /// <summary>FicheNo تهاتر در رایورز بدون اسلش است (040933318150 نه 040933/318150).</summary>
    public static string NormalizeFicheNo(string? ficheNo)
    {
        var f = (ficheNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(f))
            throw new ArgumentException("شماره فیش تهاتر الزامی است.");
        return f.Replace("/", "", StringComparison.Ordinal);
    }

    /// <summary>
    /// یک ردیف مبلغ تهاتر مطابق Tahator1:
    /// Price=(-1)*Payable ؛ WrapperAccountNo=iif(CI_Bank=4,200098,200099) ؛
    /// Center / Center1 / Center3 از RefParameetrs همان تابع.
    /// </summary>
    public static void ApplyTahatorRows(FicheHeaderDto fiche)
    {
        if (fiche.Category != FicheCategory.Income)
            return;

        if (!string.IsNullOrWhiteSpace(fiche.FicheNo))
            fiche.FicheNo = NormalizeFicheNo(fiche.FicheNo);
        TahatorResendService.ApplyTahatorDocTyp(fiche);

        var bank = (fiche.BankCode ?? "").Trim();
        // Tahator1: TmpInComeCode = iif(CI_Bank=4, 200098, 200099) ; fileN = iif(CI_Bank=4, 4, 2)
        var incmNo = bank == "4" ? IncmNoBank4 : IncmNoOther;
        var fileNo = bank == "4" ? 4 : 2;

        // Tahator1 RefParameetrs "Center":
        //   if CI_Bank="2" Then CreditorPapers.ToString() Else "0"
        fiche.Center = bank == "2"
            ? (fiche.CreditorPapers ?? 0)
            : 0;

        var district = ResolveDistrictBranchFromNosaziCode(fiche.BnkAcntNo);
        if (district > 0)
        {
            fiche.ResolvedDistrictBranch = district;
            var fund = ResolveTahatorFund(district);
            if (fund > 0)
                fiche.SuggestedFund = fund;
        }

        // Tahator1: Center1 = deposit — Center2 در Tahator1 اصلاً ست نمی‌شود
        // Tahator1: if CheckNo="5" Then Center3=700100002 Else Center3=700100001
        var center3 = string.Equals(fiche.CheckNo?.Trim(), "5", StringComparison.Ordinal)
            ? Center3CheckNo5
            : Center3Default;

        fiche.Rows = new List<IncmRowDto>
        {
            new()
            {
                IncmNo = incmNo,
                Val = -Math.Abs(fiche.Payable),
                IncmRowDsc = "مبلغ تهاتر",
                Center1 = fiche.Deposit,
                Center2 = null,
                Center3 = center3,
                // Tahator1: Ref = DepositID
                Ref = fiche.DepositId?.ToString(),
                Num = fileNo.ToString()
            }
        };
    }

    /// <summary>Tahator1: جمع ردیف = (−1)×Payable.</summary>
    public static bool RowSumMatchesPayable(FicheHeaderDto fiche, decimal rowSum)
    {
        if (IsTahatorFiche(fiche) || fiche.DocTyp is 14 or 15)
            return rowSum == -Math.Abs(fiche.Payable);
        return rowSum == fiche.Payable;
    }
}

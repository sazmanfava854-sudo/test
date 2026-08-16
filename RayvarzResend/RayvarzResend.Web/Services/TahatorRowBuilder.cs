using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;

namespace RayvarzResend.Web.Services;

/// <summary>
/// تهاتر — دو مسیر از Member 1388:
/// <list type="bullet">
/// <item><c>Tahator1</c> / <c>CI_IncomeAccountGroup=157</c> «مبلغ تهاتر» → ارسال به <b>مرکز</b> (Branch=102)، DocTyp ۱۴/۱۵</item>
/// <item><c>Tahator</c> / <c>CI_IncomeAccountGroup=158</c> «درآمدی تهاتر» → ارسال به <b>منطقه</b> (Branch=۲۰۱–۲۱۲)، DocTyp ۱۷/۱۸</item>
/// </list>
/// DocTyp داخل هر مسیر با <c>CI_Bank</c> انتخاب می‌شود (۴→۱۴|۱۷ ، غیر۴→۱۵|۱۸).
/// </summary>
public static class TahatorRowBuilder
{
    /// <summary>مبلغ تهاتر — تابع Tahator1 — مرکز</summary>
    public const int IncomeAccountGroupTahatorAmount = 157;
    /// <summary>درآمدی تهاتر — تابع Tahator — منطقه</summary>
    public const int IncomeAccountGroupTahatorIncome = 158;

    /// <summary>سازگاری با کد قدیمی (=157)</summary>
    public const int IncomeAccountGroupTahator = IncomeAccountGroupTahatorAmount;

    public const int IncmNoBank4 = 200098;
    public const int IncmNoOther = 200099;
    public const long Center3Default = 700100001;
    public const long Center3CheckNo5 = 700100002;

    /// <summary>Tahator (درآمدی): Center1 ثابت در نمونه‌های رایورز</summary>
    public const long TahatorIncomeCenter1 = 335000181;

    /// <summary>Branch ثابت تهاتر مبلغ (مرکز) در نمونه‌های رایورز</summary>
    public const int DefaultRayvarzBranch = 102;

    /// <summary>PhasType — مبلغ=2 (ptDraft) ؛ درآمدی=7 (مثل VB Tahator)</summary>
    public const string PhasTypCodeAmount = "2";
    public const string PhasTypCodeIncome = "7";

    /// <summary>سازگاری: پیش‌فرض مسیر مبلغ</summary>
    public const string PhasTypCode = PhasTypCodeAmount;

    /// <summary>vchrtyp — مبلغ=1 (pfPay) ؛ درآمدی=0 (مثل VB Tahator)</summary>
    public const string VchrTypCodeAmount = "1";
    public const string VchrTypCodeIncome = "0";
    public const string VchrTypCode = VchrTypCodeAmount;

    /// <summary>نمونه اصلی: ActTyp = 1 (هر دو مسیر)</summary>
    public const string ActTypCode = "1";

    /// <summary>Branch ارسال تهاتر — ۱۵۷→۱۰۲؛ ۱۵۸→منطقه (۲۰۱–۲۱۲)؛ بدون fallback خاموش به مرکز.</summary>
    public static int ResolveSendBranch(FicheHeaderDto fiche, int requestBranch)
    {
        if (requestBranch > 0) return requestBranch;
        if (IsTahatorIncomeFiche(fiche))
        {
            if (fiche.ResolvedDistrictBranch is > 0)
                return fiche.ResolvedDistrictBranch.Value;
            throw new InvalidOperationException(
                $"منطقه تهاتر درآمدی (۱۵۸) برای فیش {fiche.FicheNo} مشخص نیست — BnkAcntNo یا ResolvedDistrictBranch خالی است.");
        }

        return DefaultRayvarzBranch;
    }

    public static string ResolvePhasTypCode(FicheHeaderDto fiche) =>
        IsTahatorIncomeFiche(fiche) ? PhasTypCodeIncome : PhasTypCodeAmount;

    public static string ResolveVchrTypCode(FicheHeaderDto fiche) =>
        IsTahatorIncomeFiche(fiche) ? VchrTypCodeIncome : VchrTypCodeAmount;

    /// <summary>گروه ۱۵۷ اولویت دارد؛ در غیر این صورت DocTyp ۱۴/۱۵.</summary>
    public static bool IsTahatorAmountFiche(FicheHeaderDto fiche)
    {
        if (fiche.Category != FicheCategory.Income) return false;
        if (fiche.IncomeAccountGroup == IncomeAccountGroupTahatorAmount) return true;
        if (fiche.IncomeAccountGroup == IncomeAccountGroupTahatorIncome) return false;
        return fiche.DocTyp is 14 or 15;
    }

    /// <summary>گروه ۱۵۸ اولویت دارد؛ در غیر این صورت DocTyp ۱۷/۱۸.</summary>
    public static bool IsTahatorIncomeFiche(FicheHeaderDto fiche)
    {
        if (fiche.Category != FicheCategory.Income) return false;
        if (fiche.IncomeAccountGroup == IncomeAccountGroupTahatorIncome) return true;
        if (fiche.IncomeAccountGroup == IncomeAccountGroupTahatorAmount) return false;
        return fiche.DocTyp is 17 or 18;
    }

    public static bool IsTahatorFiche(FicheHeaderDto fiche) =>
        IsTahatorAmountFiche(fiche) || IsTahatorIncomeFiche(fiche);

    /// <summary>
    /// Fund تهاتر مبلغ (مرکز) — Tahator1 RefFund: 201→51 … 218→63
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
    /// Fund تهاتر درآمدی (منطقه) از نمونه‌های incmdocsys: 201→31 … 212→42
    /// </summary>
    public static int ResolveTahatorIncomeFund(int districtBranch) =>
        districtBranch switch
        {
            201 => 31,
            202 => 32,
            203 => 33,
            204 => 34,
            205 => 35,
            206 => 36,
            207 => 37,
            208 => 38,
            209 => 39,
            210 => 40,
            211 => 41,
            212 => 42,
            218 => 43,
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

    /// <summary>
    /// فقط Trim — اسلش را حذف نکن و واریانت نساز.
    /// </summary>
    public static string NormalizeFicheNo(string? ficheNo)
    {
        var f = (ficheNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(f))
            throw new ArgumentException("شماره فیش تهاتر الزامی است.");
        return f;
    }

    /// <summary>
    /// DocTyp بر اساس گروه کاربری + CI_Bank.
    /// مبلغ(۱۵۷): Bank=4→۱۴ وگرنه ۱۵ — درآمدی(۱۵۸): Bank=4→۱۷ وگرنه ۱۸.
    /// </summary>
    public static void ApplyTahatorDocTyp(FicheHeaderDto fiche)
    {
        var bank = (fiche.BankCode ?? "").Trim();
        // گروه حساب اولویت دارد؛ DocTyp فقط وقتی گروه مشخص نیست
        var incomePath = IsTahatorIncomeFiche(fiche);

        if (incomePath)
        {
            // VB Tahator: Mess.docdsc / DocTypDsc
            fiche.DocTyp = bank == "4" ? 17 : 18;
            fiche.DocDsc = "اسناد تهاتر درامد";
            fiche.DocTypDsc = "عوارض تهاتر درامد";
        }
        else
        {
            // VB Tahator1
            fiche.DocTyp = bank == "4" ? 14 : 15;
            fiche.DocDsc = "اسناد تهاتر مبلغ";
            fiche.DocTypDsc = "تهاتر مبلغ";
        }

        fiche.Category = FicheCategory.Income;
    }

    /// <summary>مسیر مناسب بر اساس CI_IncomeAccountGroup / DocTyp.</summary>
    public static void ApplyTahatorRows(FicheHeaderDto fiche)
    {
        if (fiche.Category != FicheCategory.Income)
            return;

        if (!string.IsNullOrWhiteSpace(fiche.FicheNo))
            fiche.FicheNo = fiche.FicheNo.Trim();

        if (IsTahatorIncomeFiche(fiche)
            || fiche.IncomeAccountGroup == IncomeAccountGroupTahatorIncome)
        {
            ApplyTahatorIncomeRows(fiche);
            return;
        }

        ApplyTahatorAmountRows(fiche);
    }

    /// <summary>
    /// Tahator1 — مبلغ تهاتر — ارسال به مرکز (Branch=102):
    /// Val=(-1)*Payable ؛ IncmNo=iif(CI_Bank=4,200098,200099).
    /// </summary>
    public static void ApplyTahatorAmountRows(FicheHeaderDto fiche)
    {
        if (fiche.IncomeAccountGroup <= 0)
            fiche.IncomeAccountGroup = IncomeAccountGroupTahatorAmount;

        ApplyTahatorDocTyp(fiche);

        var bank = (fiche.BankCode ?? "").Trim();
        var incmNo = bank == "4" ? IncmNoBank4 : IncmNoOther;
        var fileNo = bank == "4" ? 4 : 2;

        fiche.Center = bank == "2"
            ? (fiche.CreditorPapers ?? 0)
            : 0;

        ApplyDistrictAndFund(fiche, amountPath: true);

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
                Ref = fiche.DepositId?.ToString(),
                Num = fileNo.ToString()
            }
        };
    }

    /// <summary>
    /// Tahator — درآمدی تهاتر — ارسال به منطقه:
    /// یک ردیف با Val=+Payable؛ IncmNo=iif(CI_Bank=4,200098,200099) — مثل Tahator1 ولی مثبت.
    /// </summary>
    public static void ApplyTahatorIncomeRows(FicheHeaderDto fiche) =>
        ApplyTahatorIncomeCenters(fiche);

    /// <summary>یک ردیف تهاتر درآمدی — نه Income_Calculation (آتش‌نشانی/ماده۹/زیربنا).</summary>
    public static void ApplyTahatorIncomeCenters(FicheHeaderDto fiche)
    {
        fiche.IncomeAccountGroup = IncomeAccountGroupTahatorIncome;
        ApplyTahatorDocTyp(fiche);

        var bank = (fiche.BankCode ?? "").Trim();
        fiche.Center = bank == "2"
            ? (fiche.CreditorPapers ?? 0)
            : 0;

        ApplyDistrictAndFund(fiche, amountPath: false);

        if (fiche.Payable <= 0)
        {
            fiche.Rows.Clear();
            return;
        }

        var incmNo = bank == "4" ? IncmNoBank4 : IncmNoOther;
        var refVal = bank == "4" ? "4" : "2";
        fiche.Rows =
        [
            new IncmRowDto
            {
                IncmNo = incmNo,
                Val = Math.Abs(fiche.Payable),
                IncmRowDsc = "عوارض تهاتر درامد",
                Center1 = TahatorIncomeCenter1,
                Center2 = null,
                Center3 = null,
                Ref = refVal,
                Num = fiche.DepositId?.ToString()
            }
        ];
    }

    private static void ApplyDistrictAndFund(FicheHeaderDto fiche, bool amountPath)
    {
        var district = ResolveDistrictBranchFromNosaziCode(fiche.BnkAcntNo);
        if (district <= 0 && fiche.ResolvedDistrictBranch is > 0)
            district = fiche.ResolvedDistrictBranch.Value;
        if (district <= 0)
            return;

        fiche.ResolvedDistrictBranch = district;
        var fund = amountPath
            ? ResolveTahatorFund(district)
            : ResolveTahatorIncomeFund(district);
        if (fund > 0)
            fiche.SuggestedFund = fund;
    }

    /// <summary>
    /// مبلغ: جمع = (−1)×Payable ؛ درآمدی: جمع = +Payable.
    /// </summary>
    public static bool RowSumMatchesPayable(FicheHeaderDto fiche, decimal rowSum)
    {
        if (IsTahatorAmountFiche(fiche) || fiche.DocTyp is 14 or 15)
            return rowSum == -Math.Abs(fiche.Payable);
        if (IsTahatorIncomeFiche(fiche) || fiche.DocTyp is 17 or 18)
            return rowSum == Math.Abs(fiche.Payable);
        if (fiche.IncomeAccountGroup == 151)
            return rowSum == -Math.Abs(fiche.Payable);
        return rowSum == fiche.Payable;
    }

    /// <summary>ردیف مبلغ تهاتر از Sara/PrepareTahatorFiche آماده شده — از اعمال مجدد در موتور جلوگیری کن.</summary>
    public static bool IsTahatorAmountRowsPrepared(FicheHeaderDto fiche) =>
        IsTahatorAmountFiche(fiche)
        && fiche.Rows.Count == 1
        && fiche.Rows[0].IncmNo is IncmNoBank4 or IncmNoOther
        && fiche.Rows[0].Val <= 0;

    /// <summary>ردیف درآمد تهاتر از TahatorRowBuilder آماده شده — یک ردیف 200098/200099.</summary>
    public static bool IsTahatorIncomeRowsPrepared(FicheHeaderDto fiche) =>
        IsTahatorIncomeFiche(fiche)
        && fiche.Rows.Count == 1
        && fiche.Rows[0].IncmNo is IncmNoBank4 or IncmNoOther
        && fiche.Rows[0].Center1 == TahatorIncomeCenter1
        && fiche.Rows[0].Val > 0;
}

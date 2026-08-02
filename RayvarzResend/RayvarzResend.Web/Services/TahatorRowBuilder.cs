using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ساخت ردیف SOAP تهاتر مطابق تابع Tahator1 در Member 1388 (گروه حساب 157).
/// </summary>
public static class TahatorRowBuilder
{
    public const int IncomeAccountGroupTahator = 157;
    public const int IncmNoBank4 = 200098;
    public const int IncmNoOther = 200099;
    public const long Center3Default = 700100001;
    public const long Center3CheckNo5 = 700100002;

    public static bool IsTahatorFiche(FicheHeaderDto fiche) =>
        fiche.Category == FicheCategory.Income
        && (fiche.IncomeAccountGroup == IncomeAccountGroupTahator
            || fiche.DocTyp is 14 or 15);

    /// <summary>
    /// یک ردیف مبلغ تهاتر: Val = -Payable، Centers از Deposit / CheckNo / CI_Bank.
    /// </summary>
    public static void ApplyTahatorRows(FicheHeaderDto fiche)
    {
        if (fiche.Category != FicheCategory.Income)
            return;

        TahatorResendService.ApplyTahatorDocTyp(fiche);

        var bank = (fiche.BankCode ?? "").Trim();
        var incmNo = bank == "4" ? IncmNoBank4 : IncmNoOther;
        var fileNo = bank == "4" ? 4 : 2;

        // DocumentItem.Center: Bank=2 → CreditorPapers وگرنه 0 (در نمونه‌ها NULL≈0)
        fiche.Center = bank == "2" && fiche.CreditorPapers is > 0
            ? fiche.CreditorPapers
            : 0;

        // Center3: CheckNo=5 → 700100002 وگرنه 700100001
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

    /// <summary>برای تهاتر جمع ردیف‌ها = −Payable است.</summary>
    public static bool RowSumMatchesPayable(FicheHeaderDto fiche, decimal rowSum)
    {
        if (IsTahatorFiche(fiche) || fiche.DocTyp is 14 or 15)
            return rowSum == -Math.Abs(fiche.Payable);
        return rowSum == fiche.Payable;
    }
}

using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>پروفایل ردیف‌سازی هر تابع درآمد Member 1388 — تسک ۳.</summary>
public static class Member1388IncomeRowProfiles
{
    private static readonly Member1388IncomeRowOptions IncomeOptions = new()
    {
        ApplyOddments = true,
        ApplyBedeHi = true,
        ExcludeBedeHiWhenAccountGroups = [125, 126],
        RowNum = "1",
        RowNumWhenAccountGroup = 150
    };

    private static readonly Member1388IncomeRowOptions HavaleTOptions = new()
    {
        ApplyOddments = true,
        ApplyBedeHi = true,
        BedeHiWhenAccountGroup = 152,
        RowNum = "2",
        RowNumWhenAccountGroup = 152
    };

    private static readonly Member1388IncomeRowOptions OraghOptions = new()
    {
        ApplyOddments = true,
        ApplyBedeHi = true,
        BedeHiWhenAccountGroup = 154,
        RowNum = "4",
        RowNumWhenAccountGroup = 154
    };

    private static readonly Member1388IncomeRowOptions BazAfarineOptions = new()
    {
        ApplyOddments = true,
        ApplyBedeHi = false,
        RowNumFromDepositId = true
    };

    private static readonly Member1388IncomeRowOptions SeprdehScaleOptions = new()
    {
        ApplyOddments = true,
        ApplyBedeHi = false
    };

    public static void ApplyIncome(FicheHeaderDto fiche) =>
        Member1388IncomeRowBuilderCore.ApplyStandard(fiche, IncomeOptions);

    public static void ApplyHavaleT(FicheHeaderDto fiche) =>
        Member1388IncomeRowBuilderCore.ApplyStandard(fiche, HavaleTOptions);

    public static void ApplyOragh(FicheHeaderDto fiche)
    {
        if (fiche.Rows.Count == 0)
            return;

        Member1388IncomeRowBuilderCore.ApplyStandard(fiche, OraghOptions);
    }

    public static void ApplyGhatarShahri(FicheHeaderDto fiche) =>
        Member1388IncomeRowBuilderCore.ApplyPrimaryRowOnly(
            fiche, Member1388IncomeRowBuilderCore.GhatarPrimaryIncmNo);

    public static void ApplySeprdeh(FicheHeaderDto fiche)
    {
        if (HasPrimaryCalculationRow(fiche, Member1388IncomeRowBuilderCore.SeprdehPrimaryIncmNo))
            Member1388IncomeRowBuilderCore.ApplyPrimaryRowOnly(
                fiche, Member1388IncomeRowBuilderCore.SeprdehPrimaryIncmNo);
        else
            Member1388IncomeRowBuilderCore.ApplyStandard(fiche, SeprdehScaleOptions);
    }

    public static void ApplyEshghal(FicheHeaderDto fiche) =>
        Member1388IncomeRowBuilderCore.ApplyPrimaryRowOnly(
            fiche, Member1388IncomeRowBuilderCore.EshghalPrimaryIncmNo);

    public static void ApplyBackSeprdeh(FicheHeaderDto fiche) =>
        Member1388IncomeRowBuilderCore.ApplyBackSeprdeh(fiche);

    public static void ApplyBazAfarine(FicheHeaderDto fiche) =>
        Member1388IncomeRowBuilderCore.ApplyStandard(fiche, BazAfarineOptions);

    private static bool HasPrimaryCalculationRow(FicheHeaderDto fiche, int incmNo) =>
        fiche.Rows.Any(r => r.IncmNo == incmNo && r.Val != 0);
}

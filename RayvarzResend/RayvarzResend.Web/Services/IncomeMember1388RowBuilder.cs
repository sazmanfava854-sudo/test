using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;

namespace RayvarzResend.Web.Services;

/// <summary>
/// نرمال‌سازی ردیف درآمد — parity کامل Member 1388 (Oddment، BedeHi، scale، reconcile).
/// </summary>
public static class IncomeMember1388RowBuilder
{
    public static void Apply(FicheHeaderDto fiche)
    {
        if (fiche.Category != FicheCategory.Income)
            return;

        var group = fiche.IncomeAccountGroup ?? 0;

        if (group == TahatorRowBuilder.IncomeAccountGroupTahatorAmount)
        {
            TahatorRowBuilder.ApplyTahatorAmountRows(fiche);
            return;
        }

        if (group == TahatorRowBuilder.IncomeAccountGroupTahatorIncome)
        {
            TahatorRowBuilder.ApplyTahatorIncomeRows(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOMEBackSeprdeh", fiche))
        {
            Member1388IncomeRowProfiles.ApplyBackSeprdeh(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOMEHavaleT", fiche))
        {
            Member1388IncomeRowProfiles.ApplyHavaleT(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOMEGhatar_Shahri", fiche))
        {
            Member1388IncomeRowProfiles.ApplyGhatarShahri(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOMEOragh", fiche))
        {
            Member1388IncomeRowProfiles.ApplyOragh(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOMESeprdeh", fiche))
        {
            Member1388IncomeRowProfiles.ApplySeprdeh(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("IncomeHoushmand", fiche))
        {
            Member1388SpecialIncomeRowBuilder.ApplyHoushmand(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("IncomeSrvElectronic", fiche))
        {
            Member1388SpecialIncomeRowBuilder.ApplySrvElectronic(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("BazAfarineOld", fiche)
            && fiche.Rows.Any(r => Member1388BazAfarineOldRowBuilder.AllowedIncomeCodes.Contains(r.IncmNo)))
        {
            Member1388BazAfarineOldRowBuilder.Apply(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("BazAfarine", fiche))
        {
            Member1388IncomeRowProfiles.ApplyBazAfarine(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOMEEshghal", fiche))
        {
            Member1388IncomeRowProfiles.ApplyEshghal(fiche);
            return;
        }

        if (Member1388AccountGroupRules.AppliesToFiche("iNcOME", fiche))
            Member1388IncomeRowProfiles.ApplyIncome(fiche);
    }

    /// <summary>جبران اختلاف رند — VB TmpAccounting_DocDetailsListTotal(0) قبل از SOAP.</summary>
    public static void ReconcileSoapRows(IList<IncmRowDto> rows, decimal payable)
    {
        if (rows.Count == 0)
            return;

        var diff = payable - rows.Sum(r => r.Val);
        if (diff != 0)
            rows[0].Val += diff;
    }
}

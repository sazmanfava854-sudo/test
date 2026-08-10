using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>ردیف‌های Nosazi Member 1388 — VB خطوط ۵۹–۵۴۰ + DutyNosaziLogic.</summary>
public static class Member1388NosaziRowBuilder
{
    public static bool Apply(FicheHeaderDto fiche)
    {
        if (fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi))
            return false;

        if (string.Equals(fiche.FicheNo.Trim(), "1", StringComparison.Ordinal))
            return false;

        var isSenfi = fiche.Category == FicheCategory.DutySenfi;
        var exportType = fiche.DutyExportType ?? 0;
        var rows = BuildRows(fiche, isSenfi, exportType);
        if (rows.Count == 0)
            return false;

        fiche.Rows = rows;
        ApplyMetadata(fiche, isSenfi);
        return true;
    }

    public static List<IncmRowDto> BuildRows(FicheHeaderDto fiche, bool isSenfi, int exportType)
    {
        if (fiche.DutySubs.Count == 0)
            return fiche.Rows.Count > 0 ? fiche.Rows.ToList() : [];

        var subs = fiche.DutySubs
            .Select(s => (s.DutyFormula, s.DutyFormulaFiche, s.Price))
            .ToList();

        DutyOddmentLogic.ApplyToSubs(subs, fiche.DutyOddments, fiche.FicheNo);

        var amounts = DutyNosaziLogic.CalculateSubAmounts(subs, fiche.Payable);
        return DutyNosaziLogic.BuildIncmRows(amounts, isSenfi, exportType);
    }

    private static void ApplyMetadata(FicheHeaderDto fiche, bool isSenfi)
    {
        var branch = DutyDistrictBranchResolver.ResolveBranch(fiche.BillIdRaw, fiche.PaymentIdRaw);
        if (branch > 0)
            fiche.ResolvedDistrictBranch = branch;

        var bankCode = DutyNosaziLogic.DefaultBankCode(fiche.BankCode ?? fiche.PaymentBranch);
        fiche.BankCode = bankCode;
        fiche.PaymentBranch = bankCode;

        if (branch > 0)
            fiche.SuggestedFund = DutyDistrictBranchResolver.ResolveFund(branch, bankCode);

        fiche.BillId = DutyNosaziLogic.NormalizeMergedId(
            string.IsNullOrWhiteSpace(fiche.BillId) ? fiche.BillIdRaw : fiche.BillId);
        fiche.PaymentId = DutyNosaziLogic.NormalizeMergedId(
            string.IsNullOrWhiteSpace(fiche.PaymentId) ? fiche.PaymentIdRaw : fiche.PaymentId);

        if (isSenfi)
        {
            fiche.BnkAcntNo = "7-14-55-1-1-0-1";
            fiche.BnkAcntNoSource = "کد ثابت صنفی — Member 1388 Nosazi";
        }

        DutyNosaziLogic.ApplyRayvarzDates(
            fiche,
            fiche.CurrentStatus,
            fiche.PaymentDate ?? fiche.RayvarzActDate,
            fiche.BankPaymentDate ?? fiche.RayvarzDocDate);

        foreach (var row in fiche.Rows)
            row.Center1 = 0;
    }
}

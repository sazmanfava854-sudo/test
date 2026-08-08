using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تعیین Branch/Fund از منطقه فیش — همان منطق UI.</summary>
public static class FicheBranchResolver
{
    public static (int Branch, int Fund) Resolve(FicheHeaderDto fiche)
    {
        if (fiche.ResolvedDistrictBranch is > 0)
        {
            var branch = fiche.ResolvedDistrictBranch.Value;
            var fund = fiche.SuggestedFund
                ?? DutyDistrictBranchResolver.ResolveFund(branch, fiche.BankCode ?? "18");
            return (branch, fund);
        }

        var regionStr = fiche.DutyRegion ?? fiche.IncomeRegion;
        if (!string.IsNullOrWhiteSpace(regionStr) && int.TryParse(regionStr.Trim(), out var region))
        {
            var branch = region == 218 ? 218 : region is >= 1 and <= 12 ? 200 + region : 0;
            if (branch > 0)
            {
                var fund = DutyDistrictBranchResolver.ResolveFund(branch, fiche.BankCode ?? "18");
                return (branch, fund);
            }
        }

        return (201, 200201012);
    }
}

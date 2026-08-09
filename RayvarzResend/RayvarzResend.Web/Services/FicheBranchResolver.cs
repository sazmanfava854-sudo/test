using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تعیین Branch/Fund از منطقه فیش — همان منطق UI.</summary>
public static class FicheBranchResolver
{
    public const string RegionNotResolvedMessage =
        "منطقه/شعبه از فیش قابل تشخیص نیست — ارسال متوقف شد.";

    public static bool TryResolve(FicheHeaderDto fiche, out int branch, out int fund, out string? error)
    {
        branch = 0;
        fund = 0;
        error = null;

        if (fiche.ResolvedDistrictBranch is > 0)
        {
            branch = fiche.ResolvedDistrictBranch.Value;
            fund = fiche.SuggestedFund
                ?? DutyDistrictBranchResolver.ResolveFund(branch, fiche.BankCode ?? "18");
            if (fund <= 0)
            {
                error = RegionNotResolvedMessage;
                return false;
            }

            return true;
        }

        var regionStr = fiche.DutyRegion ?? fiche.IncomeRegion;
        if (!string.IsNullOrWhiteSpace(regionStr) && int.TryParse(regionStr.Trim(), out var region))
        {
            var mapped = MapRegionToBranch(region);
            if (mapped > 0)
            {
                branch = mapped;
                fund = DutyDistrictBranchResolver.ResolveFund(branch, fiche.BankCode ?? "18");
                if (fund <= 0)
                {
                    error = RegionNotResolvedMessage;
                    return false;
                }

                return true;
            }
        }

        error = RegionNotResolvedMessage;
        return false;
    }

    internal static int MapRegionToBranch(int region) =>
        region switch
        {
            218 or 80 => 218,
            >= 1 and <= 12 => 200 + region,
            >= 201 and <= 212 => region,
            _ => 0
        };
}

using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>نگاشت Center/Fund منطقه‌ای از VB Member 1388 — iNcOMEEshghal، iNcOMESeprdeh، BazAfarine و …</summary>
public static class Member1388IncomeCenterResolver
{
    public const int OraghFund = 120;
    public const int HavaleTFund = 120;
    public const int GhatarShahriFund = 265325013;
    public const long BazAfarineCenter1 = 335000046;
    public const long BazAfarineCenter2 = 800800007;
    public const int DefaultRegionalFund = 1200;

    public static int ResolveDistrictBranch(FicheHeaderDto fiche)
    {
        if (fiche.ResolvedDistrictBranch is > 0)
            return fiche.ResolvedDistrictBranch.Value;

        var fromNosazi = TahatorRowBuilder.ResolveDistrictBranchFromNosaziCode(fiche.BnkAcntNo);
        if (fromNosazi > 0)
            return fromNosazi;

        var fromBill = DutyDistrictBranchResolver.ResolveBranch(fiche.BillIdRaw, fiche.PaymentIdRaw);
        if (fromBill > 0)
            return fromBill;

        var regionStr = fiche.DutyRegion ?? fiche.IncomeRegion;
        if (!string.IsNullOrWhiteSpace(regionStr) && int.TryParse(regionStr.Trim(), out var region))
        {
            var mapped = FicheBranchResolver.MapRegionToBranch(region);
            if (mapped > 0)
                return mapped;
        }

        return 0;
    }

    /// <summary>Center2 اشغال معابر / برگشت سپرده — VB خطوط ۳۰۲۵–۳۰۵۳.</summary>
    public static long ResolveCenter2Eshghal(int districtBranch) =>
        districtBranch switch
        {
            1 or 201 => 5519510,
            2 or 202 => 5519520,
            3 or 203 => 5519530,
            4 or 204 => 5519540,
            5 or 205 => 5519550,
            6 or 206 => 5519560,
            7 or 207 => 5519570,
            8 or 208 => 5519580,
            9 or 209 => 5519590,
            10 or 210 => 5519600,
            11 or 211 => 5519610,
            12 or 212 => 5519630,
            80 or 218 => 5519620,
            _ => DefaultRegionalFund
        };

    /// <summary>Fund سپرده/اشغال — VB خطوط ۲۵۹۲–۲۶۱۰ و مشابه.</summary>
    public static int ResolveSeprdehFund(int districtBranch) =>
        districtBranch switch
        {
            1 or 201 => 200201021,
            2 or 202 => 200202017,
            3 or 203 => 200203021,
            4 or 204 => 200204018,
            5 or 205 => 200205016,
            6 or 206 => 200206016,
            7 or 207 => 200207017,
            8 or 208 => 200208017,
            9 or 209 => 200209016,
            10 or 210 => 200210022,
            11 or 211 => 200211015,
            12 or 212 => 200212016,
            80 or 218 => 200218028,
            _ => DefaultRegionalFund
        };

    /// <summary>Center/Center3 بازآفرینی — VB خطوط ۷۲۸۲–۷۳۰۷.</summary>
    public static long? ResolveRegionalCenter910(int districtBranch) =>
        districtBranch switch
        {
            1 or 201 => 910100001,
            2 or 202 => 910200001,
            3 or 203 => 910300001,
            4 or 204 => 910400001,
            5 or 205 => 910500001,
            6 or 206 => 910600001,
            7 or 207 => 910700001,
            8 or 208 => 910800001,
            9 or 209 => 910900001,
            10 or 210 => 911000001,
            11 or 211 => 911100001,
            12 or 212 => 911200001,
            80 or 218 => 911300001,
            _ => null
        };

    public static void ApplyCenter1FromDeposit(FicheHeaderDto fiche)
    {
        if (fiche.Deposit is not > 0)
            return;

        foreach (var row in fiche.Rows)
            row.Center1 = fiche.Deposit;
    }

    public static void ApplyOragh(FicheHeaderDto fiche)
    {
        ApplyCenter1FromDeposit(fiche);
        fiche.SuggestedFund = OraghFund;
        SetRowNum(fiche, "4");
    }

    public static void ApplyHavaleT(FicheHeaderDto fiche)
    {
        ApplyCenter1FromDeposit(fiche);
        fiche.SuggestedFund = HavaleTFund;
        SetRowNum(fiche, "2");
    }

    public static void ApplyGhatarShahri(FicheHeaderDto fiche)
    {
        ApplyCenter1FromDeposit(fiche);
        fiche.SuggestedFund = GhatarShahriFund;
    }

    public static void ApplySeprdeh(FicheHeaderDto fiche)
    {
        ApplyCenter1FromDeposit(fiche);
        var branch = ResolveDistrictBranch(fiche);
        if (branch > 0)
            fiche.SuggestedFund = ResolveSeprdehFund(branch);
    }

    public static void ApplyEshghal(FicheHeaderDto fiche)
    {
        ApplyCenter1FromDeposit(fiche);
        var branch = ResolveDistrictBranch(fiche);
        if (branch <= 0)
            return;

        var center2 = ResolveCenter2Eshghal(branch);
        foreach (var row in fiche.Rows)
            row.Center2 = center2;
        fiche.SuggestedFund = ResolveSeprdehFund(branch);
    }

    public static void ApplyBazAfarine(FicheHeaderDto fiche)
    {
        var branch = ResolveDistrictBranch(fiche);
        var regional = branch > 0 ? ResolveRegionalCenter910(branch) : null;
        if (regional is > 0)
            fiche.Center = regional;

        foreach (var row in fiche.Rows)
        {
            row.Center1 = BazAfarineCenter1;
            row.Center2 = BazAfarineCenter2;
            if (regional is > 0)
                row.Center3 = regional;
        }
    }

    private static void SetRowNum(FicheHeaderDto fiche, string num)
    {
        foreach (var row in fiche.Rows)
            row.Num = num;
    }
}

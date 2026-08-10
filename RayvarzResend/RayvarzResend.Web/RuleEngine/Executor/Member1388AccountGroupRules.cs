using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>
/// شرط ExistRayvarz=False در ابتدای هر تابع Member 1388 — استخراج از VB paste.
/// Run همه Callها را می‌زند؛ هر تابع فقط وقتی گروه حساب منطبق است اثر می‌گذارد.
/// </summary>
public static class Member1388AccountGroupRules
{
    private static readonly HashSet<int> IncomeCityPlanningGroups =
    [
        1, 7, 8, 10, 11, 15, 22, 29, 36, 43, 50, 57, 64, 71, 78,
        125, 126, 150, 152, 161, 162
    ];

    private static readonly HashSet<int> EshghalGroups =
    [
        7, 14, 19, 21, 24, 28, 35, 42, 46, 49, 63, 70, 77, 84, 85, 102, 120, 124
    ];

    public static bool AppliesToFiche(string functionName, FicheHeaderDto fiche)
    {
        if (functionName.Equals("Nosazi", StringComparison.OrdinalIgnoreCase))
            return fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi;

        if (fiche.Category != FicheCategory.Income)
            return false;

        var g = fiche.IncomeAccountGroup ?? 0;
        var key = functionName.Replace("_", "", StringComparison.Ordinal).ToUpperInvariant();
        return key switch
        {
            "INCOME" => IncomeCityPlanningGroups.Contains(g),
            "INCOMEORAGH" => g == 154,
            "INCOMESEPRDEH" => g is >= 130 and <= 142 or 155,
            "INCOMEESHGHAL" => g == 124 || EshghalGroups.Contains(g),
            "INCOMEGHATARSHAHRI" => g == 153,
            "INCOMEBACKSEPRDEH" => g == 151,
            "INCOMEHAVALET" => g == 152,
            "BAZAFARINE" or "BAZAFARINEOLD" => g == 156,
            "TAHATOR1" => g == 157,
            "TAHATOR" => g == 158,
            "INCOMEHOUSHMAND" => g == 163,
            "INCOMESRVELECTRONIC" => g == 164,
            "INCOMECHECK" => true,
            "BEDEHI" => true,
            _ => false
        };
    }

    public static string SkipReason(string functionName, FicheHeaderDto fiche)
    {
        var g = fiche.IncomeAccountGroup;
        return $"تابع {functionName} برای CI_IncomeAccountGroup={g ?? 0} اعمال نمی‌شود";
    }
}

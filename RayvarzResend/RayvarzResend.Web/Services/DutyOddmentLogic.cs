using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعمال رندمان Duty_OddmentAccount روی Duty_FicheSub — همان الگوی IncomeOddmentLogic.</summary>
public static class DutyOddmentLogic
{
    private static readonly HashSet<int> SubtractTypes = [2, 3, 6, 7];
    private static readonly HashSet<int> AddTypes = [8, 1, 4];

    public static void ApplyToSubs(
        IList<(int Formula, int Fiche, decimal Price)> subs,
        IReadOnlyList<DutyOddmentDto> oddments,
        string? currentFicheNo)
    {
        if (oddments.Count == 0)
            return;

        var scoped = ScopeForFiche(oddments, currentFicheNo);
        for (var i = 0; i < subs.Count; i++)
        {
            var (formula, fiche, price) = subs[i];
            subs[i] = (formula, fiche, ApplyNetAdjustment(price, scoped, formula, fiche));
        }

        AppendMissingOddmentSubs(subs, scoped);
    }

    public static decimal ApplyNetAdjustment(
        decimal basePrice,
        IReadOnlyList<DutyOddmentDto> oddments,
        int dutyFormula,
        int dutyFormulaFiche)
    {
        var subtract = oddments
            .Where(o => o.DutyFormula == dutyFormula && o.DutyFormulaFiche == dutyFormulaFiche
                        && SubtractTypes.Contains(o.OddmentType))
            .Sum(o => o.Price);
        var add = oddments
            .Where(o => o.DutyFormula == dutyFormula && o.DutyFormulaFiche == dutyFormulaFiche
                        && AddTypes.Contains(o.OddmentType))
            .Sum(o => o.Price);
        return basePrice - subtract + add;
    }

    public static void AppendMissingOddmentSubs(
        IList<(int Formula, int Fiche, decimal Price)> subs,
        IReadOnlyList<DutyOddmentDto> oddments)
    {
        var existing = subs.Select(s => (s.Formula, s.Fiche)).ToHashSet();
        foreach (var odd in oddments)
        {
            var key = (odd.DutyFormula, odd.DutyFormulaFiche);
            if (existing.Contains(key))
                continue;

            var price = SubtractTypes.Contains(odd.OddmentType) ? -odd.Price : odd.Price;
            if (price == 0)
                continue;

            subs.Add((odd.DutyFormula, odd.DutyFormulaFiche, price));
            existing.Add(key);
        }
    }

    private static List<DutyOddmentDto> ScopeForFiche(
        IReadOnlyList<DutyOddmentDto> oddments,
        string? currentFicheNo)
    {
        if (string.IsNullOrWhiteSpace(currentFicheNo))
            return oddments.ToList();

        var trimmed = currentFicheNo.Trim();
        return oddments
            .Where(o => string.IsNullOrWhiteSpace(o.FicheNo)
                        || o.FicheNo.Trim().Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

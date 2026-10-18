using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;

namespace RayvarzResend.Web.Services;

/// <summary>ساخت ردیف DTL تهاتر درآمدی — parity VB Tahator (TmpDT + oddment + scale به Payable).</summary>
public static class TahatorIncomeCalculationBuilder
{
    public static void Apply(FicheHeaderDto fiche)
    {
        if (fiche.Payable <= 0)
        {
            fiche.Rows.Clear();
            return;
        }

        var working = fiche.Rows
            .Where(r => r.IncmNo > 0 && r.Val != 0)
            .Select(CloneRow)
            .ToList();

        if (working.Count == 0)
            return;

        IncomeOddmentLogic.ApplyToRows(working, fiche.Oddments, fiche.NidIncome);
        AppendTahatorOddmentRows(working, fiche.Oddments, fiche.IncomeCodeByCalculation);

        working = working.Where(r => r.IncmNo > 0 && r.Val != 0).ToList();
        if (working.Count == 0)
        {
            fiche.Rows.Clear();
            return;
        }

        ScaleToPayableLikeVbTahator(working, fiche.Payable);
        working = MergeRowsByIncmNo(working);

        fiche.Rows.Clear();
        fiche.Rows.AddRange(working);
    }

    private static void ScaleToPayableLikeVbTahator(IList<IncmRowDto> rows, decimal payable)
    {
        var baseSum = rows.Sum(r => r.Val);
        if (baseSum == 0)
            return;

        foreach (var row in rows)
        {
            if (row.Val == 0)
                continue;
            row.Val = Math.Round(row.Val * payable / baseSum, 0, MidpointRounding.AwayFromZero);
        }

        // VB: TmpAccounting_DocDetailsListTotal(0).Price = SaraPrice - sum(1..n)
        if (rows.Count > 0)
        {
            var tail = rows.Skip(1).Sum(r => r.Val);
            rows[0].Val = payable - tail;
        }
    }

    private static List<IncmRowDto> MergeRowsByIncmNo(IEnumerable<IncmRowDto> rows)
    {
        return rows
            .GroupBy(r => r.IncmNo)
            .Select(g =>
            {
                var first = g.First();
                return new IncmRowDto
                {
                    IncmNo = g.Key,
                    IncmRowDsc = g.Select(x => x.IncmRowDsc).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? first.IncmRowDsc,
                    Val = g.Sum(x => x.Val),
                    Center1 = first.Center1,
                    Center2 = first.Center2,
                    Center3 = first.Center3,
                    Ref = first.Ref,
                    Num = first.Num
                };
            })
            .Where(r => r.Val != 0)
            .OrderBy(r => r.IncmNo)
            .ToList();
    }

    private static void AppendTahatorOddmentRows(
        IList<IncmRowDto> rows,
        IReadOnlyList<IncomeOddmentDto> oddments,
        IReadOnlyDictionary<int, string>? incomeCodeByCalculation)
    {
        if (oddments.Count == 0)
            return;

        var existing = rows.Select(r => r.IncmNo).ToHashSet();
        foreach (var odd in oddments)
        {
            if (IncomeExcludedCodes.Codes.Contains(odd.IncmNo))
                continue;

            var incomeCode = incomeCodeByCalculation != null && incomeCodeByCalculation.TryGetValue(odd.IncmNo, out var c)
                ? c
                : null;
            if (IncomeRayvarzIncmNoResolver.IsMetaIncomeCode(incomeCode))
                continue;

            var rayIncm = IncomeRayvarzIncmNoResolver.Resolve(odd.IncmNo, incomeCode);
            if (rayIncm <= 0 || existing.Contains(rayIncm))
                continue;

            var subtract = odd.OddmentType is 2 or 3 or 6 or 7;
            var price = subtract ? -odd.Value : odd.Value;
            if (price == 0)
                continue;

            rows.Add(new IncmRowDto
            {
                IncmNo = rayIncm,
                Val = price,
                IncmRowDsc = $"Oddment-{odd.IncmNo}"
            });
            existing.Add(rayIncm);
        }
    }

    private static IncmRowDto CloneRow(IncmRowDto source) => new()
    {
        IncmNo = source.IncmNo,
        IncmRowDsc = source.IncmRowDsc,
        Val = source.Val,
        Center1 = source.Center1,
        Center2 = source.Center2,
        Center3 = source.Center3,
        Ref = source.Ref,
        Num = source.Num
    };
}

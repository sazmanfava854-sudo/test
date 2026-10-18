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
            // VB Tahator LstOdd_1: WrapperAccountNo = CI_IncomeCalculation؛ فقط IncomeCode 0/1 رد می‌شود.
            if (IncomeRayvarzIncmNoResolver.IsMetaIncomeCode(incomeCode))
                continue;

            var incmNo = odd.IncmNo;
            if (incmNo <= 0 || existing.Contains(incmNo))
                continue;

            var subtract = odd.OddmentType is 2 or 3 or 6 or 7;
            var price = subtract ? -odd.Value : odd.Value;
            if (price == 0)
                continue;

            rows.Add(new IncmRowDto
            {
                IncmNo = incmNo,
                Val = price,
                IncmRowDsc = $"Oddment-{odd.IncmNo}"
            });
            existing.Add(incmNo);
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

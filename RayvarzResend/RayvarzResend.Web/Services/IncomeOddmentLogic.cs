using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>رندمان Income_OddmentAccount — VB LstOdd / LstOdd_1 در iNcOMEOragh.</summary>
public static class IncomeOddmentLogic
{
    private static readonly HashSet<int> SubtractTypes = [2, 3, 6, 7];
    private static readonly HashSet<int> AddTypes = [8, 1, 4];

    public static void ApplyToRows(IList<IncmRowDto> rows, IReadOnlyList<IncomeOddmentDto> oddments, Guid? nidIncome)
    {
        if (oddments.Count == 0)
            return;

        foreach (var row in rows)
        {
            if (IncomeExcludedCodes.Codes.Contains(row.IncmNo))
                continue;

            row.Val = ApplyNetAdjustment(row.Val, oddments, row.IncmNo);
        }

        AppendMissingOddmentRows(rows, oddments);
    }

    public static decimal ApplyNetAdjustment(
        decimal baseValue,
        IReadOnlyList<IncomeOddmentDto> oddments,
        int incmNo)
    {
        var subtract = oddments
            .Where(o => o.IncmNo == incmNo && SubtractTypes.Contains(o.OddmentType))
            .Sum(o => o.Value);
        var add = oddments
            .Where(o => o.IncmNo == incmNo && AddTypes.Contains(o.OddmentType))
            .Sum(o => o.Value);
        return baseValue - subtract + add;
    }

    public static void AppendMissingOddmentRows(IList<IncmRowDto> rows, IReadOnlyList<IncomeOddmentDto> oddments)
    {
        var existing = rows.Select(r => r.IncmNo).ToHashSet();
        foreach (var odd in oddments)
        {
            if (IncomeExcludedCodes.Codes.Contains(odd.IncmNo))
                continue;
            if (existing.Contains(odd.IncmNo))
                continue;

            var price = SubtractTypes.Contains(odd.OddmentType) ? -odd.Value : odd.Value;
            if (price == 0)
                continue;

            rows.Add(new IncmRowDto
            {
                IncmNo = odd.IncmNo,
                Val = price,
                IncmRowDsc = $"Oddment-{odd.IncmNo}"
            });
            existing.Add(odd.IncmNo);
        }
    }
}

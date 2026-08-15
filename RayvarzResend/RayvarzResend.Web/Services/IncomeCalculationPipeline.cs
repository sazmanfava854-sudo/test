using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>فیلتر Income_Calculation + نرمال‌سازی Member 1388.</summary>
public static class IncomeCalculationPipeline
{
    public static List<IncmRowDto> PrepareRows(
        IEnumerable<IncmRowDto> rawRows,
        decimal payable,
        int incomeAccountGroup = 150,
        IReadOnlyList<IncomeOddmentDto>? oddments = null)
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = incomeAccountGroup,
            Payable = payable,
            Oddments = oddments?.ToList() ?? [],
            Rows = rawRows
                .Select(r => new IncmRowDto
                {
                    IncmNo = r.IncmNo,
                    Val = r.Val,
                    IncmRowDsc = r.IncmRowDsc
                })
                .Where(r => r.Val != 0)
                .ToList()
        };

        IncomeMember1388RowBuilder.Apply(fiche);
        return fiche.Rows;
    }
}

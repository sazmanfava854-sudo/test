using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>فیلتر Income_Calculation + اسکیل به Payable — مطابق Member1388 و ray.incmdocsys.</summary>
public static class IncomeCalculationPipeline
{
    public static List<IncmRowDto> PrepareRows(IEnumerable<IncmRowDto> rawRows, decimal payable)
    {
        var rows = rawRows
            .Where(r => !IncomeExcludedCodes.Codes.Contains(r.IncmNo) && r.Val != 0)
            .Select(r => new IncmRowDto
            {
                IncmNo = r.IncmNo,
                Val = r.Val,
                IncmRowDsc = r.IncmRowDsc
            })
            .ToList();

        IncomeRowScaler.ScaleToPayable(rows, payable);
        return rows;
    }
}

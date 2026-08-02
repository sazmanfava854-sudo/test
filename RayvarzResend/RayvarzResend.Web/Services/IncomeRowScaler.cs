using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// هم‌تراز با SoapBuilder.NormalizeRows برای Income:
/// جمع خام Income_Calculation (قبل از تخفیف) را به PayablePrice اسکیل می‌کند.
/// </summary>
public static class IncomeRowScaler
{
    public static void ScaleToPayable(IList<IncmRowDto> rows, decimal payable)
    {
        if (rows.Count == 0) return;

        var sum = rows.Sum(r => r.Val);
        if (sum == payable || sum == 0) return;

        var factor = payable / sum;
        foreach (var r in rows)
            r.Val = Math.Round(r.Val * factor, 0);

        var diff = payable - rows.Sum(r => r.Val);
        rows[0].Val += diff;
    }
}

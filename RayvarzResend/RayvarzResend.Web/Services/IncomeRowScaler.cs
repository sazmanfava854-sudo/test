using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// سازگاری با مسیر قدیمی — منطق واقعی در <see cref="IncomeMember1388RowBuilder"/>.
/// </summary>
public static class IncomeRowScaler
{
    public static void ScaleToPayable(IList<IncmRowDto> rows, decimal payable)
    {
        if (rows.Count == 0) return;

        var sum = rows.Sum(r => r.Val);
        if (sum == payable || sum == 0) return;
        if (sum == -payable) return;

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 150,
            Payable = payable,
            Rows = rows.Select(Clone).ToList()
        };

        IncomeMember1388RowBuilder.Apply(fiche);

        rows.Clear();
        foreach (var row in fiche.Rows)
            rows.Add(row);
    }

    private static IncmRowDto Clone(IncmRowDto source) => new()
    {
        IncmNo = source.IncmNo,
        Val = source.Val,
        IncmRowDsc = source.IncmRowDsc,
        Center1 = source.Center1,
        Center2 = source.Center2,
        Center3 = source.Center3,
        Ref = source.Ref,
        Num = source.Num
    };
}

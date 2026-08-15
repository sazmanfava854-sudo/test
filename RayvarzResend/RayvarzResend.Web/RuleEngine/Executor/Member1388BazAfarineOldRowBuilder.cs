using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>
/// بازافرینی قدیم — فقط کدهای 100098/100107/100108 با مبلغ Payable (VB BazAfarineOld).
/// مستقل از BazAfarine که از IncomeExcludedCodes استفاده می‌کند.
/// </summary>
public static class Member1388BazAfarineOldRowBuilder
{
    public static readonly HashSet<int> AllowedIncomeCodes = [100098, 100107, 100108];

    public static void Apply(FicheHeaderDto fiche)
    {
        if (fiche.Payable <= 0)
        {
            fiche.Rows.Clear();
            return;
        }

        var candidates = fiche.Rows
            .Where(r => AllowedIncomeCodes.Contains(r.IncmNo))
            .Select(CloneRow)
            .ToList();

        if (candidates.Count == 0)
        {
            fiche.Rows =
            [
                new IncmRowDto
                {
                    IncmNo = 100098,
                    Val = fiche.Payable,
                    IncmRowDsc = "بازافرینی قدیم"
                }
            ];
            return;
        }

        foreach (var row in candidates)
            row.Val = fiche.Payable;

        fiche.Rows.Clear();
        fiche.Rows.AddRange(candidates);
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

using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>بدهی قبلی Member 1388 — VB BedeHi (خطوط ۴۸۱۲–۴۸۷۰).</summary>
public static class Member1388BedeHiHelper
{
    public static decimal Resolve(DslExecutionContext context, FicheHeaderDto fiche, int districtBranch)
    {
        if (fiche.PriorBedeHiAmount.HasValue)
            return fiche.PriorBedeHiAmount.Value;

        var prior = fiche.PriorIncomeFiche
                    ?? SelectPriorFromContext(context, districtBranch, fiche.FicheNo);

        if (prior is not null)
            fiche.PriorIncomeFiche ??= prior;

        return BedeHiLogic.Resolve(districtBranch, fiche.FicheNo, prior);
    }

    public static decimal Resolve(FicheHeaderDto fiche, int districtBranch) =>
        Resolve(new DslExecutionContext { Fiche = fiche }, fiche, districtBranch);

    private static PriorIncomeFicheDto? SelectPriorFromContext(
        DslExecutionContext context,
        int districtBranch,
        string currentFicheNo)
    {
        if (!context.Variables.TryGetValue(BedeHiLogic.PriorCandidatesKey, out var raw))
            return null;

        return raw switch
        {
            IEnumerable<PriorIncomeFicheDto> list => BedeHiLogic.SelectPrior(list, districtBranch, currentFicheNo),
            PriorIncomeFicheDto single => BedeHiLogic.SelectPrior([single], districtBranch, currentFicheNo),
            _ => null
        };
    }
}

using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>
/// ردیف‌های iNcOMEOragh — Oddment، BedeHi، مقیاس‌دهی به Payable.
/// VB: member-1388-full-body.vb خطوط ۱۷۳۵–۲۱۰۷.
/// </summary>
public static class Member1388OraghRowBuilder
{
  public static void Apply(FicheHeaderDto fiche) =>
    Member1388IncomeRowProfiles.ApplyOragh(fiche);

  public static decimal ResolveBedeHiAmount(FicheHeaderDto fiche, int district)
  {
    if (fiche.PriorBedeHiAmount.HasValue)
      return fiche.PriorBedeHiAmount.Value;

    return BedeHiLogic.Resolve(district, fiche.FicheNo, fiche.PriorIncomeFiche);
  }

}

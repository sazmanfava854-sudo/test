using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>ردیف‌های IncomeHoushmand و IncomeSrvElectronic — VB خطوط ۸۵۸۰–۸۹۱۳.</summary>
public static class Member1388SpecialIncomeRowBuilder
{
  public const int BaseIncmNo = 401312100;
  public const int VatIncmNo = 206098003;
  public const int Branch682 = 682;
  public const int Fund682 = 200682010;

  public static void ApplyHoushmand(FicheHeaderDto fiche)
  {
    if (fiche.Payable <= 0)
    {
      fiche.Rows.Clear();
      return;
    }

    var basePrice = Math.Round(fiche.Payable / 1.1m, 0);
    var vat = Math.Round(basePrice * 0.1m, 0);
  fiche.Rows =
    [
      new IncmRowDto
      {
        IncmNo = BaseIncmNo,
        Val = basePrice,
        IncmRowDsc = "بهای هوشمندسازی خدمات شهری"
      },
      new IncmRowDto
      {
        IncmNo = VatIncmNo,
        Val = vat,
        IncmRowDsc = "ارزش افزوده"
      }
    ];

    ReconcileToPayable(fiche.Rows, fiche.Payable);
    Member1388IncomeCenterResolver.ApplyHoushmand(fiche);
  }

  public static void ApplySrvElectronic(FicheHeaderDto fiche)
  {
    if (fiche.Payable <= 0)
    {
      fiche.Rows.Clear();
      return;
    }

    var basePrice = Math.Round(fiche.Payable, 0);
    var vat = Math.Round(basePrice * 0.1m, 0);
    fiche.Rows =
    [
      new IncmRowDto
      {
        IncmNo = BaseIncmNo,
        Val = basePrice,
        IncmRowDsc = "بهاي خدمات سرويس هاي الكترونيك شهرداري"
      },
      new IncmRowDto
      {
        IncmNo = VatIncmNo,
        Val = vat,
        IncmRowDsc = "ارزش افزوده"
      }
    ];

    ReconcileToPayable(fiche.Rows, fiche.Payable);
    Member1388IncomeCenterResolver.ApplySrvElectronic(fiche);
  }

  private static void ReconcileSum(IList<IncmRowDto> rows, decimal target)
  {
    if (rows.Count == 0)
      return;

    var diff = target - rows.Sum(r => r.Val);
    if (diff != 0)
      rows[0].Val += diff;
  }

  private static void ReconcileToPayable(IList<IncmRowDto> rows, decimal payable) =>
    ReconcileSum(rows, payable);
}

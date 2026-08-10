using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class DutyOddmentLogicTests
{
  [Fact]
  public void ApplyToSubs_subtracts_and_adds_by_oddment_type()
  {
    var subs = new List<(int Formula, int Fiche, decimal Price)>
    {
      (5, 0, 1_000_000m)
    };

    DutyOddmentLogic.ApplyToSubs(subs,
    [
      new DutyOddmentDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 100_000m, OddmentType = 2 },
      new DutyOddmentDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 50_000m, OddmentType = 4 }
    ],
    "F001");

    Assert.Equal(950_000m, subs[0].Price);
  }

  [Fact]
  public void ApplyToSubs_appends_missing_formula_rows()
  {
    var subs = new List<(int Formula, int Fiche, decimal Price)>
    {
      (5, 0, 1_000_000m)
    };

    DutyOddmentLogic.ApplyToSubs(subs,
    [
      new DutyOddmentDto { DutyFormula = 3, DutyFormulaFiche = 0, Price = 200_000m, OddmentType = 1 }
    ],
    "F001");

    Assert.Equal(2, subs.Count);
    Assert.Contains(subs, s => s.Formula == 3 && s.Fiche == 0 && s.Price == 200_000m);
  }

  [Fact]
  public void ApplyToSubs_filters_by_fiche_no_when_set()
  {
    var subs = new List<(int Formula, int Fiche, decimal Price)> { (5, 0, 1_000_000m) };

    DutyOddmentLogic.ApplyToSubs(subs,
    [
      new DutyOddmentDto
      {
        DutyFormula = 5, DutyFormulaFiche = 0, Price = 100_000m, OddmentType = 2, FicheNo = "OTHER"
      },
      new DutyOddmentDto
      {
        DutyFormula = 5, DutyFormulaFiche = 0, Price = 50_000m, OddmentType = 2, FicheNo = "F001"
      }
    ],
    "F001");

    Assert.Equal(950_000m, subs[0].Price);
  }

  [Fact]
  public void ApplyToSubs_affects_nosazi_row_amounts_via_calculate_sub_amounts()
  {
    var subs = new List<(int Formula, int Fiche, decimal Price)>
    {
      (5, 0, 300_000m),
      (3, 0, 200_000m),
      (3, 16, 100_000m)
    };

    DutyOddmentLogic.ApplyToSubs(subs,
    [
      new DutyOddmentDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 50_000m, OddmentType = 2 }
    ],
    "F001");

    var amounts = DutyNosaziLogic.CalculateSubAmounts(subs, 1_000_000m);
    var rows = DutyNosaziLogic.BuildIncmRows(amounts, isSenfi: false, exportType: 0);

    Assert.Equal(250_000m, amounts.Atash);
    Assert.Equal(450_000m, amounts.MainLine);
    Assert.Contains(rows, r => r.IncmNo == 100002 && r.Val == 250_000m);
  }
}

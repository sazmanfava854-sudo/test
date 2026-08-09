using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>
/// ردیف‌های iNcOMEOragh — Oddment، BedeHi، مقیاس‌دهی به Payable.
/// VB: member-1388-full-body.vb خطوط ۱۷۳۵–۲۱۰۷.
/// </summary>
public static class Member1388OraghRowBuilder
{
  public static void Apply(FicheHeaderDto fiche)
  {
    if (fiche.Rows.Count == 0)
      return;

    var working = fiche.Rows
      .Select(CloneRow)
      .Where(r => !IncomeExcludedCodes.Codes.Contains(r.IncmNo))
      .ToList();

    IncomeOddmentLogic.ApplyToRows(working, fiche.Oddments, fiche.NidIncome);

    var district = Member1388IncomeCenterResolver.ResolveDistrictBranch(fiche);
    var bedeHi = ResolveBedeHiAmount(fiche, district);
    fiche.PriorBedeHiAmount = bedeHi;

    var debtRows = bedeHi > 0 && fiche.IncomeAccountGroup == 154
      ? BuildBedeHiDebtRows(fiche, bedeHi)
      : [];

    var baseSum = IncomeOddmentLogic.SumEligibleRows(working);
    if (baseSum == 0 && debtRows.Count == 0)
    {
      fiche.Rows.Clear();
      return;
    }

    var scaleTarget = fiche.Payable + bedeHi;
    if (baseSum > 0)
    {
      foreach (var row in working)
      {
        if (row.Val == 0)
          continue;
        row.Val = Math.Round(row.Val * scaleTarget / baseSum, 0);
      }

      ReconcileSum(working, scaleTarget);
    }

    var combined = working.Where(r => r.Val != 0).Concat(debtRows).ToList();
    if (combined.Count == 0)
      return;

    ReconcileSum(combined, fiche.Payable);

    foreach (var row in combined)
      row.Num = "4";

    fiche.Rows.Clear();
    fiche.Rows.AddRange(combined);
  }

  public static decimal ResolveBedeHiAmount(FicheHeaderDto fiche, int district)
  {
    if (fiche.PriorBedeHiAmount.HasValue)
      return fiche.PriorBedeHiAmount.Value;

    return BedeHiLogic.Resolve(district, fiche.FicheNo, fiche.PriorIncomeFiche);
  }

  private static List<IncmRowDto> BuildBedeHiDebtRows(FicheHeaderDto fiche, decimal bedeHi)
  {
    var sourceRows = fiche.PriorIncomeFiche?.CalculationRows is { Count: > 0 } priorRows
      ? priorRows
      : fiche.Rows;

    var eligible = sourceRows
      .Where(r => !IncomeExcludedCodes.Codes.Contains(r.IncmNo))
      .Select(CloneRow)
      .Where(r => r.Val != 0)
      .ToList();

    var total = eligible.Sum(r => r.Val);
    if (total <= 0)
      return [];

    var debtRows = new List<IncmRowDto>();
    for (var i = 0; i < eligible.Count; i++)
    {
      var src = eligible[i];
      var price = -Math.Round(src.Val * bedeHi / total, 0);
      if (price == 0)
        continue;

      debtRows.Add(new IncmRowDto
      {
        IncmNo = src.IncmNo,
        Val = price,
        IncmRowDsc = src.IncmRowDsc,
        Num = "4"
      });
    }

    if (debtRows.Count > 0)
    {
      var debtSum = debtRows.Sum(r => r.Val);
      var targetDebt = -bedeHi;
      if (debtSum != targetDebt)
        debtRows[0].Val += targetDebt - debtSum;
    }

    return debtRows;
  }

  private static void ReconcileSum(IList<IncmRowDto> rows, decimal target)
  {
    if (rows.Count == 0)
      return;

    var diff = target - rows.Sum(r => r.Val);
    if (diff != 0)
      rows[0].Val += diff;
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

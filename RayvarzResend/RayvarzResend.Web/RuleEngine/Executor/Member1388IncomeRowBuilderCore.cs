using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>هسته مشترک Oddment + BedeHi + مقیاس‌دهی — الگوی iNcOMEOragh.</summary>
public static class Member1388IncomeRowBuilderCore
{
    public const int GhatarPrimaryIncmNo = 100101;
    public const int EshghalPrimaryIncmNo = 100006;
    public const int SeprdehPrimaryIncmNo = 120;
    public const int BackSeprdehIncmNo = 229091300;

    public static void ApplyStandard(FicheHeaderDto fiche, Member1388IncomeRowOptions options)
    {
        if (fiche.Rows.Count == 0 && fiche.Payable <= 0)
            return;

        var working = fiche.Rows.Count == 0
            ? new List<IncmRowDto>()
            : fiche.Rows
                .Select(CloneRow)
                .Where(r => !IncomeExcludedCodes.Codes.Contains(r.IncmNo))
                .ToList();

        if (options.ApplyOddments && working.Count > 0)
            IncomeOddmentLogic.ApplyToRows(working, fiche.Oddments, fiche.NidIncome);

        var district = Member1388IncomeCenterResolver.ResolveDistrictBranch(fiche);
        var bedeHi = 0m;
        if (ShouldApplyBedeHi(fiche, options))
        {
            bedeHi = Member1388OraghRowBuilder.ResolveBedeHiAmount(fiche, district);
            fiche.PriorBedeHiAmount = bedeHi;
        }

        var debtRows = bedeHi > 0
            ? BuildBedeHiDebtRows(fiche, bedeHi)
            : [];

        var baseSum = IncomeOddmentLogic.SumEligibleRows(working);
        if (baseSum == 0 && debtRows.Count == 0)
        {
            if (fiche.Payable <= 0)
            {
                fiche.Rows.Clear();
                return;
            }

            working.Add(new IncmRowDto { IncmNo = 1, Val = fiche.Payable });
            baseSum = fiche.Payable;
        }

        var scaleTarget = fiche.Payable + bedeHi;
        if (baseSum > 0 && scaleTarget > 0)
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
        ApplyRowNum(fiche, combined, options);

        fiche.Rows.Clear();
        fiche.Rows.AddRange(combined);
    }

    public static void ApplyPrimaryRowOnly(FicheHeaderDto fiche, int primaryIncmNo, string? rowNum = null)
    {
        if (fiche.Payable <= 0)
        {
            fiche.Rows.Clear();
            return;
        }

        var match = fiche.Rows.FirstOrDefault(r => r.IncmNo == primaryIncmNo);
        if (match is not null)
        {
            foreach (var row in fiche.Rows)
                row.Val = row.IncmNo == primaryIncmNo ? fiche.Payable : 0;
        }
        else
        {
            fiche.Rows =
            [
                new IncmRowDto
                {
                    IncmNo = primaryIncmNo,
                    Val = fiche.Payable
                }
            ];
        }

        if (!string.IsNullOrWhiteSpace(rowNum))
        {
            foreach (var row in fiche.Rows.Where(r => r.Val != 0))
                row.Num = rowNum;
        }
    }

    public static void ApplyBackSeprdeh(FicheHeaderDto fiche)
    {
        if (fiche.Payable <= 0)
        {
            fiche.Rows.Clear();
            return;
        }

        fiche.Rows =
        [
            new IncmRowDto
            {
                IncmNo = BackSeprdehIncmNo,
                Val = -fiche.Payable,
                IncmRowDsc = "برگشت از سپرده",
                Num = "3"
            }
        ];
    }

    private static bool ShouldApplyBedeHi(FicheHeaderDto fiche, Member1388IncomeRowOptions options)
    {
        if (!options.ApplyBedeHi)
            return false;

        var group = fiche.IncomeAccountGroup ?? 0;
        if (options.ExcludeBedeHiWhenAccountGroups.Contains(group))
            return false;

        if (options.BedeHiWhenAccountGroup.HasValue)
            return group == options.BedeHiWhenAccountGroup.Value;

        return true;
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
        foreach (var src in eligible)
        {
            var price = -Math.Round(src.Val * bedeHi / total, 0);
            if (price == 0)
                continue;

            debtRows.Add(new IncmRowDto
            {
                IncmNo = src.IncmNo,
                Val = price,
                IncmRowDsc = src.IncmRowDsc
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

    private static void ApplyRowNum(
        FicheHeaderDto fiche,
        IList<IncmRowDto> rows,
        Member1388IncomeRowOptions options)
    {
        string? num = null;
        if (options.RowNumFromDepositId && fiche.DepositId is > 0)
            num = fiche.DepositId.Value.ToString();
        else if (options.RowNumWhenAccountGroup.HasValue
                 && fiche.IncomeAccountGroup == options.RowNumWhenAccountGroup.Value)
            num = options.RowNum;
        else if (!options.RowNumWhenAccountGroup.HasValue && !string.IsNullOrWhiteSpace(options.RowNum))
            num = options.RowNum;

        if (num is null)
            return;

        foreach (var row in rows)
            row.Num = num;
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

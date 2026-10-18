using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class TahatorIncomeCalculationBuilderTests
{
    [Theory]
    [InlineData(1271, "1270", 1270)]
    [InlineData(1271, "", 1271)]
    [InlineData(1025, "0", 0)]
    [InlineData(100116, "100116", 100116)]
    public void IncomeRayvarzIncmNoResolver_maps_IncomeCode_for_non_tahator_paths(int calcId, string incomeCode, int expected)
    {
        Assert.Equal(expected, IncomeRayvarzIncmNoResolver.Resolve(calcId, incomeCode));
    }

    [Fact]
    public void Tahator_builder_scales_without_merging_duplicate_incm_no()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            Payable = 1_000_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1271, Val = 600_000m, IncmRowDsc = "زیربنا" },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m, IncmRowDsc = "زیربنا" },
                new IncmRowDto { IncmNo = 100116, Val = 200_000m, IncmRowDsc = "ماده 9" }
            }
        };

        TahatorIncomeCalculationBuilder.Apply(fiche);

        Assert.Equal(3, fiche.Rows.Count);
        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(2, fiche.Rows.Count(r => r.IncmNo == 1271));
    }

    [Fact]
    public void Tahator_builder_first_row_reconciles_rounding_like_vb()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 100m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1271, Val = 33m },
                new IncmRowDto { IncmNo = 100116, Val = 33m },
                new IncmRowDto { IncmNo = 1288, Val = 34m }
            }
        };

        TahatorIncomeCalculationBuilder.Apply(fiche);

        Assert.Equal(100m, fiche.Rows.Sum(r => r.Val));
    }

    /// <summary>فیش تهاتر 050833533237 — parity ray.incmdocsys (۶ ردیف، IncmNo = CI_IncomeCalculation).</summary>
    [Fact]
    public void Tahator_fiche_050833533237_keeps_six_calculation_incm_nos()
    {
        const decimal payable = 2_350_000_000m;
        var gross = new[]
        {
            (1025, 2_001_734_525m),
            (1275, 159_795_492m),
            (1267, 4_027_447m),
            (1288, 9_674_041m),
            (100116, 16_123_402m),
            (1271, 158_645_093m),
        };

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            FicheNo = "050833533237",
            Payable = payable,
            Rows = gross.Select(g => new IncmRowDto { IncmNo = g.Item1, Val = g.Item2 }).ToList()
        };

        Member1388IncomeRowProfiles.ApplyTahatorIncome(fiche);

        Assert.Equal(6, fiche.Rows.Count);
        Assert.Equal(payable, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(
            gross.Select(g => g.Item1).OrderBy(x => x),
            fiche.Rows.Select(r => r.IncmNo).OrderBy(x => x));
        Assert.Contains(fiche.Rows, r => r.IncmNo == 1271);
        Assert.Contains(fiche.Rows, r => r.IncmNo == 1288);
        Assert.DoesNotContain(fiche.Rows, r => r.IncmNo == 1270);
        Assert.DoesNotContain(fiche.Rows, r => r.IncmNo == 1278);
        Assert.All(fiche.Rows, r => Assert.Equal(TahatorRowBuilder.TahatorIncomeCenter1, r.Center1));
    }
}

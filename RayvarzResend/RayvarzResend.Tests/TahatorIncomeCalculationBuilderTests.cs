using RayvarzResend.Web.Models;
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
    public void IncomeRayvarzIncmNoResolver_maps_IncomeCode(int calcId, string incomeCode, int expected)
    {
        Assert.Equal(expected, IncomeRayvarzIncmNoResolver.Resolve(calcId, incomeCode));
    }

    [Fact]
    public void Tahator_builder_scales_and_merges_by_rayvarz_incm_no()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            Payable = 1_000_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1270, Val = 600_000m, IncmRowDsc = "زیربنا" },
                new IncmRowDto { IncmNo = 1270, Val = 400_000m, IncmRowDsc = "زیربنا" },
                new IncmRowDto { IncmNo = 100116, Val = 200_000m, IncmRowDsc = "ماده 9" }
            }
        };

        TahatorIncomeCalculationBuilder.Apply(fiche);

        Assert.Equal(2, fiche.Rows.Count);
        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(833_333m, fiche.Rows.Single(r => r.IncmNo == 1270).Val);
        Assert.Equal(166_667m, fiche.Rows.Single(r => r.IncmNo == 100116).Val);
    }

    [Fact]
    public void Tahator_builder_first_row_reconciles_rounding_like_vb()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 100m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1270, Val = 33m },
                new IncmRowDto { IncmNo = 100116, Val = 33m },
                new IncmRowDto { IncmNo = 1278, Val = 34m }
            }
        };

        TahatorIncomeCalculationBuilder.Apply(fiche);

        Assert.Equal(100m, fiche.Rows.Sum(r => r.Val));
    }
}

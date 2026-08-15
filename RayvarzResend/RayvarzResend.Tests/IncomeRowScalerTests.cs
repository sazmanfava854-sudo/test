using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class IncomeRowScalerTests
{
    [Fact]
    public void ScaleToPayable_matches_member1388_and_sums_to_payable()
    {
        var rows = new List<IncmRowDto>
        {
            new() { IncmNo = 100116, Val = 94400000m },
            new() { IncmNo = 1025, Val = 3783000000m },
            new() { IncmNo = 1271, Val = 1744000000m },
            new() { IncmNo = 1288, Val = 37760000m },
            new() { IncmNo = 1267, Val = 143851424m }
        };
        var payable = 5_379_066_000m;
        Assert.NotEqual(payable, rows.Sum(r => r.Val));

        IncomeRowScaler.ScaleToPayable(rows, payable);

        Assert.Equal(payable, rows.Sum(r => r.Val));
        Assert.All(rows, r => Assert.True(r.Val > 0));
    }

    [Fact]
    public void ScaleToPayable_is_noop_when_already_equal()
    {
        var rows = new List<IncmRowDto>
        {
            new() { IncmNo = 1, Val = 300m },
            new() { IncmNo = 2, Val = 200m }
        };
        IncomeRowScaler.ScaleToPayable(rows, 500m);
        Assert.Equal(300m, rows[0].Val);
        Assert.Equal(200m, rows[1].Val);
    }

    [Fact]
    public void IncomeMember1388RowBuilder_applies_bedehi_scale_target()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 152,
            FicheNo = "NEW001",
            Payable = 1_000_000m,
            PriorBedeHiAmount = 200_000m,
            PriorIncomeFiche = new PriorIncomeFicheDto
            {
                FicheNo = "OLD001",
                CalculationRows =
                {
                    new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                    new IncmRowDto { IncmNo = 1271, Val = 400_000m }
                }
            },
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        IncomeMember1388RowBuilder.Apply(fiche);

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Contains(fiche.Rows, r => r.Val < 0);
    }
}

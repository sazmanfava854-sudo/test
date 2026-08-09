using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388Task3Tests
{
    [Fact]
    public void ApplyIncome_scales_rows_to_payable_and_sets_num_for_group_150()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeAccountGroup = 150,
            Payable = 1_000_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplyIncome(fiche);

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.All(fiche.Rows, r => Assert.Equal("1", r.Num));
    }

    [Fact]
    public void ApplyIncome_excludes_bedehi_for_groups_125_and_126()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeAccountGroup = 125,
            FicheNo = "NEW001",
            Payable = 1_000_000m,
            PriorBedeHiAmount = 200_000m,
            PriorIncomeFiche = new PriorIncomeFicheDto
            {
                FicheNo = "OLD001",
                CalculationRows = { new IncmRowDto { IncmNo = 1025, Val = 1_000_000m } }
            },
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 1_000_000m } }
        };

        Member1388IncomeRowProfiles.ApplyIncome(fiche);

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.DoesNotContain(fiche.Rows, r => r.Val < 0);
    }

    [Fact]
    public void ApplyIncome_applies_oddment_before_scaling()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeAccountGroup = 150,
            Payable = 900_000m,
            Oddments = { new IncomeOddmentDto { IncmNo = 1025, Value = 100_000m, OddmentType = 2 } },
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplyIncome(fiche);

        Assert.Equal(900_000m, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void ApplyHavaleT_applies_bedehi_and_sets_num_2_for_group_152()
    {
        var fiche = new FicheHeaderDto
        {
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

        Member1388IncomeRowProfiles.ApplyHavaleT(fiche);

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Contains(fiche.Rows, r => r.Val < 0);
        Assert.All(fiche.Rows, r => Assert.Equal("2", r.Num));
    }

    [Fact]
    public void ApplyGhatarShahri_assigns_payable_to_primary_incm_no_only()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 500_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = Member1388IncomeRowBuilderCore.GhatarPrimaryIncmNo, Val = 100_000m },
                new IncmRowDto { IncmNo = 1025, Val = 400_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplyGhatarShahri(fiche);

        Assert.Equal(500_000m, fiche.Rows.Single(r => r.IncmNo == Member1388IncomeRowBuilderCore.GhatarPrimaryIncmNo).Val);
        Assert.Equal(0m, fiche.Rows.Single(r => r.IncmNo == 1025).Val);
        Assert.Equal(500_000m, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void ApplySeprdeh_uses_primary_row_when_incm_120_exists()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 750_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = Member1388IncomeRowBuilderCore.SeprdehPrimaryIncmNo, Val = 100_000m },
                new IncmRowDto { IncmNo = 1025, Val = 400_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplySeprdeh(fiche);

        Assert.Equal(750_000m, fiche.Rows.Single(r => r.IncmNo == Member1388IncomeRowBuilderCore.SeprdehPrimaryIncmNo).Val);
        Assert.Equal(0m, fiche.Rows.Single(r => r.IncmNo == 1025).Val);
    }

    [Fact]
    public void ApplySeprdeh_scales_rows_when_primary_incm_120_missing()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 1_000_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplySeprdeh(fiche);

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void ApplyEshghal_assigns_payable_to_primary_incm_no_only()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 300_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = Member1388IncomeRowBuilderCore.EshghalPrimaryIncmNo, Val = 50_000m },
                new IncmRowDto { IncmNo = 1025, Val = 250_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplyEshghal(fiche);

        Assert.Equal(300_000m, fiche.Rows.Single(r => r.IncmNo == Member1388IncomeRowBuilderCore.EshghalPrimaryIncmNo).Val);
        Assert.Equal(300_000m, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void ApplyBackSeprdeh_creates_negative_single_row()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 400_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 400_000m } }
        };

        Member1388IncomeRowProfiles.ApplyBackSeprdeh(fiche);

        Assert.Single(fiche.Rows);
        Assert.Equal(Member1388IncomeRowBuilderCore.BackSeprdehIncmNo, fiche.Rows[0].IncmNo);
        Assert.Equal(-400_000m, fiche.Rows[0].Val);
        Assert.Equal("3", fiche.Rows[0].Num);
        Assert.Equal(-400_000m, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void ApplyBazAfarine_sets_num_from_deposit_id()
    {
        var fiche = new FicheHeaderDto
        {
            DepositId = 19684,
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        Member1388IncomeRowProfiles.ApplyBazAfarine(fiche);

        Assert.Equal(500_000m, fiche.Rows.Sum(r => r.Val));
        Assert.All(fiche.Rows, r => Assert.Equal("19684", r.Num));
    }

    [Fact]
    public void Execute_iNcOME_via_executor_scales_and_sets_num()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 150,
            Payable = 800_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 500_000m },
                new IncmRowDto { IncmNo = 1271, Val = 300_000m }
            }
        };

        var result = Member1388FunctionExecutor.Execute(
            "iNcOME",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(800_000m, fiche.Rows.Sum(r => r.Val));
        Assert.All(fiche.Rows, r => Assert.Equal("1", r.Num));
    }

    [Fact]
    public void Execute_iNcOMEBackSeprdeh_via_executor_builds_negative_row()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 151,
            Payable = 250_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 250_000m } }
        };

        var result = Member1388FunctionExecutor.Execute(
            "iNcOMEBackSeprdeh",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(Member1388IncomeRowBuilderCore.BackSeprdehIncmNo, fiche.Rows[0].IncmNo);
        Assert.Equal(-250_000m, fiche.Rows.Sum(r => r.Val));
    }
}

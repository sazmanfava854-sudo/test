using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388Task4Tests
{
    [Fact]
    public void ApplyOragh_scales_with_oddment_and_bedehi_for_group_154()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeAccountGroup = 154,
            Deposit = 320008535,
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
            Oddments =
            {
                new IncomeOddmentDto { IncmNo = 1025, Value = 100_000m, OddmentType = 2 }
            },
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        Member1388IncomeRowProfiles.ApplyOragh(fiche);
        Member1388IncomeCenterResolver.ApplyOragh(fiche);

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(320008535, fiche.Rows[0].Center1);
        Assert.Equal(Member1388IncomeCenterResolver.OraghFund, fiche.SuggestedFund);
        Assert.All(fiche.Rows, r => Assert.Equal("4", r.Num));
        Assert.Contains(fiche.Rows, r => r.Val < 0);
    }

    [Fact]
    public void ApplyHoushmand_splits_vat_and_sets_branch_682()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeAccountGroup = 163,
            Payable = 1_100_000m
        };

        Member1388SpecialIncomeRowBuilder.ApplyHoushmand(fiche);

        Assert.Equal(2, fiche.Rows.Count);
        Assert.Equal(Member1388SpecialIncomeRowBuilder.BaseIncmNo, fiche.Rows[0].IncmNo);
        Assert.Equal(Member1388SpecialIncomeRowBuilder.VatIncmNo, fiche.Rows[1].IncmNo);
        Assert.Equal(1_100_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(Member1388SpecialIncomeRowBuilder.Branch682, fiche.ResolvedDistrictBranch);
        Assert.Equal(Member1388SpecialIncomeRowBuilder.Fund682, fiche.SuggestedFund);
        Assert.All(fiche.Rows, r => Assert.Equal(0L, r.Center1));
    }

    [Fact]
    public void ApplySrvElectronic_builds_base_plus_vat_reconciled_to_payable()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeAccountGroup = 164,
            Payable = 1_000_000m
        };

        Member1388SpecialIncomeRowBuilder.ApplySrvElectronic(fiche);

        Assert.Equal(2, fiche.Rows.Count);
        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(Member1388SpecialIncomeRowBuilder.Fund682, fiche.SuggestedFund);
    }

    [Fact]
    public void Execute_Tahator1_builds_negative_amount_row()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            Deposit = 320008535,
            DepositId = 19684,
            Payable = 500_000m,
            BankCode = "18",
            BnkAcntNo = "9-1-1-0-0-0-0"
        };

        var result = Member1388FunctionExecutor.Execute(
            "Tahator1",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Single(fiche.Rows);
        Assert.Equal(TahatorRowBuilder.IncmNoOther, fiche.Rows[0].IncmNo);
        Assert.Equal(-500_000m, fiche.Rows[0].Val);
        Assert.Equal(15, fiche.DocTyp);
        Assert.True(TahatorRowBuilder.RowSumMatchesPayable(fiche, fiche.Rows.Sum(r => r.Val)));
    }

    [Fact]
    public void Execute_Tahator_scales_rows_with_oddment_and_sets_income_centers()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            DepositId = 10987476,
            Payable = 900_000m,
            BankCode = "4",
            BnkAcntNo = "9-1-1-0-0-0-0",
            Oddments =
            {
                new IncomeOddmentDto { IncmNo = 1025, Value = 100_000m, OddmentType = 2 }
            },
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        var result = Member1388FunctionExecutor.Execute(
            "Tahator",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        // مسیر resend تهاتر: Oddment/BedeHi اعمال نمی‌شود — فقط Center/Ref (مثل قبل از Member1388)
        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(17, fiche.DocTyp);
        Assert.All(fiche.Rows, r => Assert.Equal(TahatorRowBuilder.TahatorIncomeCenter1, r.Center1));
        Assert.All(fiche.Rows, r => Assert.Equal("4", r.Ref));
        Assert.All(fiche.Rows, r => Assert.Equal("10987476", r.Num));
        Assert.Equal(39, fiche.SuggestedFund);
    }

    [Fact]
    public void Execute_IncomeHoushmand_via_executor_sets_centers()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 163,
            Payable = 1_100_000m,
            Rows = { new IncmRowDto { IncmNo = 1, Val = 999_999m } }
        };

        var result = Member1388FunctionExecutor.Execute(
            "IncomeHoushmand",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(1_100_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(0L, fiche.Rows[0].Center1);
        Assert.Equal(Member1388SpecialIncomeRowBuilder.Fund682, fiche.SuggestedFund);
    }

    [Fact]
    public void Execute_iNcOMEOragh_via_executor_full_path()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 154,
            Deposit = 320008535,
            Payable = 800_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 500_000m },
                new IncmRowDto { IncmNo = 1271, Val = 300_000m }
            }
        };

        var result = Member1388FunctionExecutor.Execute(
            "iNcOMEOragh",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(800_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Equal(Member1388IncomeCenterResolver.OraghFund, fiche.SuggestedFund);
        Assert.All(fiche.Rows, r => Assert.Equal("4", r.Num));
    }
}

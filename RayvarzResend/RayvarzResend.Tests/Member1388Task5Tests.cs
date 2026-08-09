using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388Task5Tests
{
    [Fact]
    public void NosaziRowBuilder_builds_main_atash_garbage_afzodeh_rows()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "101104/9881711",
            Payable = 1_000_000m,
            BillIdRaw = "00000000519000000001",
            PaymentIdRaw = "00000000519000000001",
            DutySubs =
            {
                new DutySubDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 300_000m },
                new DutySubDto { DutyFormula = 3, DutyFormulaFiche = 0, Price = 200_000m },
                new DutySubDto { DutyFormula = 3, DutyFormulaFiche = 16, Price = 100_000m }
            }
        };

        Assert.True(Member1388NosaziRowBuilder.Apply(fiche));

        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Contains(fiche.Rows, r => r.IncmNo == 2003 && r.Val == 400_000m);
        Assert.Contains(fiche.Rows, r => r.IncmNo == 100002 && r.Val == 300_000m);
        Assert.Contains(fiche.Rows, r => r.IncmNo == 100003 && r.Val == 200_000m);
        Assert.Contains(fiche.Rows, r => r.IncmNo == 206098003 && r.Val == 100_000m);
        Assert.True(fiche.ResolvedDistrictBranch is > 0);
        Assert.True(fiche.SuggestedFund is > 0);
        Assert.All(fiche.Rows, r => Assert.Equal(0L, r.Center1));
    }

    [Fact]
    public void NosaziRowBuilder_skips_fiche_no_1()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "1",
            Payable = 500_000m,
            DutySubs = { new DutySubDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 500_000m } }
        };

        Assert.False(Member1388NosaziRowBuilder.Apply(fiche));
    }

    [Fact]
    public void NosaziRowBuilder_senfi_export_type_14_uses_incm_2005()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutySenfi,
            FicheNo = "SENFI001",
            Payable = 500_000m,
            DutyExportType = 14,
            DutySubs = { new DutySubDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 100_000m } }
        };

        Assert.True(Member1388NosaziRowBuilder.Apply(fiche));

        Assert.Equal(2, fiche.Rows.Count);
        Assert.Contains(fiche.Rows, r => r.IncmNo == 2005 && r.Val == 400_000m);
        Assert.Contains(fiche.Rows, r => r.IncmNo == 100002 && r.Val == 100_000m);
        Assert.Equal("7-14-55-1-1-0-1", fiche.BnkAcntNo);
    }

    [Fact]
    public void NosaziRowBuilder_preserves_preloaded_rows_when_no_duty_subs()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "101104/9881711",
            Payable = 1_000_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 2003, Val = 800_000m },
                new IncmRowDto { IncmNo = 100002, Val = 100_000m },
                new IncmRowDto { IncmNo = 100003, Val = 100_000m }
            }
        };

        Assert.True(Member1388NosaziRowBuilder.Apply(fiche));
        Assert.Equal(3, fiche.Rows.Count);
        Assert.Equal(1_000_000m, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void BedeHiLogic_selects_latest_prior_by_bank_payment_date()
    {
        var candidates = new[]
        {
            new PriorIncomeFicheDto
            {
                FicheNo = "OLD-A",
                IncomeAccountGroup = 8,
                FicheStatus = 5,
                Payable = 1_000_000m,
                Brokers = 0,
                PaymentDate = "1400/02/01",
                BankPaymentDate = "1400/02/05"
            },
            new PriorIncomeFicheDto
            {
                FicheNo = "OLD-B",
                IncomeAccountGroup = 15,
                FicheStatus = 7,
                Payable = 2_000_000m,
                Brokers = 100_000m,
                PaymentDate = "1400/03/01",
                BankPaymentDate = "1400/03/10"
            }
        };

        var prior = BedeHiLogic.SelectPrior(candidates, 209, "NEW001");

        Assert.NotNull(prior);
        Assert.Equal("OLD-B", prior!.FicheNo);
        Assert.Equal(1_900_000m, BedeHiLogic.Resolve(209, "NEW001", prior));
    }

    [Fact]
    public void BedeHiLogic_ignores_disallowed_status_and_account_group()
    {
        var candidates = new[]
        {
            new PriorIncomeFicheDto
            {
                FicheNo = "OLD-STATUS",
                IncomeAccountGroup = 8,
                FicheStatus = 2,
                Payable = 1_000_000m,
                PaymentDate = "1400/02/01",
                BankPaymentDate = "1400/02/05"
            },
            new PriorIncomeFicheDto
            {
                FicheNo = "OLD-GROUP",
                IncomeAccountGroup = 999,
                FicheStatus = 5,
                Payable = 1_000_000m,
                PaymentDate = "1400/02/01",
                BankPaymentDate = "1400/02/05"
            }
        };

        Assert.Null(BedeHiLogic.SelectPrior(candidates, 209, "NEW001"));
    }

    [Fact]
    public void BedeHiHelper_reads_prior_candidates_from_context()
    {
        var fiche = new FicheHeaderDto { FicheNo = "NEW001" };
        var context = new DslExecutionContext
        {
            Fiche = fiche,
            Variables =
            {
                [BedeHiLogic.PriorCandidatesKey] = new List<PriorIncomeFicheDto>
                {
                    new()
                    {
                        FicheNo = "OLD001",
                        IncomeAccountGroup = 8,
                        FicheStatus = 5,
                        Payable = 1_500_000m,
                        Brokers = 50_000m,
                        PaymentDate = "1400/01/01",
                        BankPaymentDate = "1400/01/15"
                    }
                }
            }
        };

        var amount = Member1388BedeHiHelper.Resolve(context, fiche, 209);

        Assert.Equal(1_450_000m, amount);
        Assert.NotNull(fiche.PriorIncomeFiche);
        Assert.Equal("OLD001", fiche.PriorIncomeFiche!.FicheNo);
    }

    [Fact]
    public void Execute_BedeHi_via_executor_sets_result_variable()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "NEW001",
            PriorIncomeFiche = new PriorIncomeFicheDto
            {
                FicheNo = "OLD001",
                IncomeAccountGroup = 8,
                Payable = 2_000_000m,
                Brokers = 200_000m,
                PaymentDate = "1400/01/01",
                BankPaymentDate = "1400/01/02"
            }
        };

        var context = new DslExecutionContext
        {
            Fiche = fiche,
            Member1388FullExecution = true,
            Variables = { ["DistrickBranch"] = 209 }
        };

        var result = Member1388FunctionExecutor.Execute(
            "BedeHi",
            context,
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(1_800_000m, context.Variables["BedeHiResult"]);
        Assert.Equal(1_800_000m, fiche.PriorBedeHiAmount);
    }

    [Fact]
    public void Execute_Nosazi_via_executor_builds_rows_from_duty_subs()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "101104/9881711",
            Payable = 600_000m,
            BillIdRaw = "00000000519000000001",
            PaymentIdRaw = "00000000519000000001",
            DutySubs =
            {
                new DutySubDto { DutyFormula = 5, DutyFormulaFiche = 0, Price = 200_000m },
                new DutySubDto { DutyFormula = 3, DutyFormulaFiche = 0, Price = 100_000m }
            }
        };

        var result = Member1388FunctionExecutor.Execute(
            "Nosazi",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(600_000m, fiche.Rows.Sum(r => r.Val));
        Assert.Contains(fiche.Rows, r => r.IncmNo == 2003);
    }
}

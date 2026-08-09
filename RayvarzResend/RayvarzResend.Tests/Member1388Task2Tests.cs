using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388Task2Tests
{
    [Fact]
    public void IncomeCheckLogic_blocks_unapproved_status()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            CurrentStatus = 2,
            Payable = 100_000m,
            BankPaymentDate = "1400/01/01"
        };

        var result = Member1388IncomeCheckLogic.Validate(fiche, new DslExecutionContext());

        Assert.False(result.Success);
        Assert.Contains("تایید نشده", result.BlockReason, StringComparison.Ordinal);
        Assert.False(fiche.CanSend);
    }

    [Fact]
    public void IncomeCheckLogic_allows_status_3()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            CurrentStatus = 3,
            Payable = 0,
            ExportPermanentDate = "1400/01/01"
        };
        var ctx = new DslExecutionContext { Variables = { ["CurrentShamsiDate"] = "1400/01/15" } };

        var result = Member1388IncomeCheckLogic.Validate(fiche, ctx);

        Assert.True(result.Success);
        Assert.True(result.HadEffect);
    }

    [Fact]
    public void IncomeCheckLogic_blocks_missing_bank_date_when_payable_positive()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            CurrentStatus = 5,
            Payable = 500_000m
        };

        var result = Member1388IncomeCheckLogic.Validate(fiche, new DslExecutionContext());

        Assert.False(result.Success);
        Assert.Contains("تایید بانک", result.BlockReason, StringComparison.Ordinal);
    }

    [Fact]
    public void IncomeCheckLogic_blocks_late_installment_by_day_diff()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            CurrentStatus = 7,
            Payable = 500_000m,
            ExportPermanentDate = "1400/01/01",
            BankPaymentDate = "1400/02/01"
        };

        var result = Member1388IncomeCheckLogic.Validate(fiche, new DslExecutionContext());

        Assert.False(result.Success);
        Assert.Contains("مهلت پرداخت", result.BlockReason, StringComparison.Ordinal);
    }

    [Fact]
    public void IncomeCheckLogic_passes_valid_installment_fiche()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            CurrentStatus = 5,
            Payable = 500_000m,
            ExportPermanentDate = "1400/01/01",
            PaymentBreakDate = "1400/01/20",
            BankPaymentDate = "1400/01/10",
            NidProc = Guid.Parse("53379B94-1411-4DC8-AEE1-012D4A9B43A7")
        };

        var result = Member1388IncomeCheckLogic.Validate(fiche, new DslExecutionContext());

        Assert.True(result.Success);
        Assert.True(fiche.CanSend);
    }

    [Fact]
    public void Run_default_chain_does_not_include_BazAfarineOld()
    {
        var order = Member1388Catalog.ResolveIncomeCallOrder(new FicheHeaderDto());
        Assert.Equal(Member1388Catalog.RunIncomeCallOrder, order);
        Assert.DoesNotContain("BazAfarineOld", order);
    }

    [Fact]
    public void Run_with_UseBazAfarineOld_swaps_bazafarine()
    {
        var order = Member1388Catalog.ResolveIncomeCallOrder(new FicheHeaderDto { UseBazAfarineOld = true });
        Assert.Contains("BazAfarineOld", order);
        Assert.DoesNotContain("BazAfarine", order);
    }

    [Fact]
    public void Execute_Run_with_UseBazAfarineOld_invokes_old_variant()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 156,
            UseBazAfarineOld = true,
            Payable = 500_000m,
            BankCode = "18",
            BnkAcntNo = "9-1-1-0-0-0-0",
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        var result = executor.Execute(program, new DslExecutionContext
        {
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Contains("BazAfarineOld", result.AppliedFunctions);
        Assert.DoesNotContain("BazAfarine", result.AppliedFunctions);
    }

    [Fact]
    public void Execute_income_check_blocks_run_when_validation_fails()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            AccountingDocumentingCause = Member1388AccountingCause.InstallmentCheck,
            CurrentStatus = 2,
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        var result = executor.Execute(program, new DslExecutionContext
        {
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.False(result.Success);
        Assert.Contains("iNcOMECheck", result.AppliedFunctions);
        Assert.Contains(result.PreSoapRuleErrors, e => e.Contains("تایید نشده", StringComparison.Ordinal));
    }

    private static DslProgram LoadFullProgram()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-full-body.vb");
        var vb = File.ReadAllText(path);
        var wrapped =
            "<?xml version=\"1.0\"?><ClsFunction><NidClass>360</NidClass><NidFunction>1388</NidFunction>" +
            $"<Name>Run</Name><Body>{System.Security.SecurityElement.Escape(vb)}</Body></ClsFunction>";
        return VbTranspiler.Transpile(XmlEnvelopeReader.Read(wrapped, "full-body").Document);
    }
}

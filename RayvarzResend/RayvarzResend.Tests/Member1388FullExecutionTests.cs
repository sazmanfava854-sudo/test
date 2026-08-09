using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388FullExecutionTests
{
    [Fact]
    public void Catalog_lists_all_23_functions_from_paste()
    {
        Assert.Equal(23, Member1388Catalog.AllFunctions.Count);
        Assert.Equal(12, Member1388Catalog.RunIncomeCallOrder.Count);
        Assert.Contains(Member1388Catalog.AllFunctions, f => f.Name == "IncomeHoushmand");
        Assert.Contains(Member1388Catalog.AllFunctions, f => f.Name == "IncomeSrvElectronic");
    }

    [Fact]
    public void Transpile_pasted_vb_finds_all_functions()
    {
        var program = LoadFullProgram();
        foreach (var def in Member1388Catalog.AllFunctions)
        {
            Assert.Contains(program.Functions, f => f.Name.Equals(def.Name, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Execute_Run_invokes_all_income_functions_from_paste()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 150,
            FicheNo = "050733453546",
            Payable = 1_000_000m,
            BankCode = "18",
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        var result = executor.Execute(program, new DslExecutionContext
        {
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(Member1388Catalog.RunIncomeCallOrder, result.AppliedFunctions);
        Assert.Contains("iNcOME", result.FunctionsWithEffect);
    }

    [Fact]
    public void Execute_Tahator1_only_when_group_157()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());

        var fiche157 = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            Payable = 500_000m,
            BankCode = "18",
            BnkAcntNo = "9-1-1-0-0-0-0"
        };
        TahatorRowBuilder.ApplyTahatorAmountRows(fiche157);

        var ctx157 = new DslExecutionContext { Fiche = fiche157, Member1388FullExecution = true };
        var r157 = Member1388FunctionExecutor.Execute("Tahator1", ctx157, SaraOperationBootstrap.CreateDefault());
        Assert.True(r157.HadEffect);

        var fiche150 = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 150,
            Payable = 500_000m
        };
        var r150 = Member1388FunctionExecutor.Execute("Tahator1", new DslExecutionContext
        {
            Fiche = fiche150,
            Member1388FullExecution = true
        }, SaraOperationBootstrap.CreateDefault());
        Assert.False(r150.HadEffect);
    }

    [Fact]
    public void RefParameterCollector_applies_centers_to_fiche()
    {
        var fiche = new FicheHeaderDto
        {
            Rows = { new IncmRowDto { IncmNo = 1, Val = 100m } }
        };
        RefParameterCollector.ApplyToFiche(fiche,
        [
            new RefParameter { Name = "Center1", Value = "320008535" },
            new RefParameter { Name = "Center3", Value = "700100001" }
        ]);

        Assert.Equal(320008535, fiche.Rows[0].Center1);
        Assert.Equal(700100001, fiche.Rows[0].Center3);
    }

    private static DslProgram LoadFullProgram()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-full-body.vb");
        Assert.True(File.Exists(path), path);
        var vb = File.ReadAllText(path);
        var wrapped =
            "<?xml version=\"1.0\"?><ClsFunction><NidClass>360</NidClass><NidFunction>1388</NidFunction>" +
            $"<Name>Run</Name><Body>{System.Security.SecurityElement.Escape(vb)}</Body></ClsFunction>";
        return VbTranspiler.Transpile(XmlEnvelopeReader.Read(wrapped, "full-body").Document);
    }
}

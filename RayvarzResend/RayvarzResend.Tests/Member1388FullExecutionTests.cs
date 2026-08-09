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

    [Theory]
    [InlineData(209, 5519590L)]
    [InlineData(9, 5519590L)]
    [InlineData(218, 5519620L)]
    [InlineData(80, 5519620L)]
    [InlineData(99, 1200L)]
    public void ResolveCenter2Eshghal_maps_district_to_vb_values(int district, long expected)
    {
        Assert.Equal(expected, Member1388IncomeCenterResolver.ResolveCenter2Eshghal(district));
    }

    [Theory]
    [InlineData(209, 200209016)]
    [InlineData(201, 200201021)]
    [InlineData(218, 200218028)]
    public void ResolveSeprdehFund_maps_district_to_vb_values(int district, int expected)
    {
        Assert.Equal(expected, Member1388IncomeCenterResolver.ResolveSeprdehFund(district));
    }

    [Fact]
    public void Execute_iNcOMEOragh_sets_center1_fund_and_file_no()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 154,
            Deposit = 320008535,
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        var result = Member1388FunctionExecutor.Execute(
            "iNcOMEOragh",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(320008535, fiche.Rows[0].Center1);
        Assert.Equal(Member1388IncomeCenterResolver.OraghFund, fiche.SuggestedFund);
        Assert.Equal("4", fiche.Rows[0].Num);
    }

    [Fact]
    public void Execute_iNcOMEEshghal_sets_regional_center2_and_fund()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 124,
            Deposit = 320008535,
            IncomeRegion = "9",
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        var result = Member1388FunctionExecutor.Execute(
            "iNcOMEEshghal",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(5519590L, fiche.Rows[0].Center2);
        Assert.Equal(200209016, fiche.SuggestedFund);
    }

    [Fact]
    public void Execute_BazAfarine_sets_regional_center_and_fixed_centers()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 156,
            BnkAcntNo = "9-1-1-0-0-0-0",
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        var result = Member1388FunctionExecutor.Execute(
            "BazAfarine",
            new DslExecutionContext { Fiche = fiche, Member1388FullExecution = true },
            SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal(910900001L, fiche.Center);
        Assert.Equal(Member1388IncomeCenterResolver.BazAfarineCenter1, fiche.Rows[0].Center1);
        Assert.Equal(Member1388IncomeCenterResolver.BazAfarineCenter2, fiche.Rows[0].Center2);
        Assert.Equal(910900001L, fiche.Rows[0].Center3);
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

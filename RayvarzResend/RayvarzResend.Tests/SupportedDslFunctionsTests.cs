using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using Xunit;

namespace RayvarzResend.Tests;

public class SupportedDslFunctionsTests
{
    [Theory]
    [InlineData("ChangeDate", DslFunctionRole.Global)]
    [InlineData("FnSMS", DslFunctionRole.Global)]
    [InlineData("GetDiffDate", DslFunctionRole.Global)]
    [InlineData("Nosazi", DslFunctionRole.Duty)]
    [InlineData("iNcOME", DslFunctionRole.Income)]
    [InlineData("IncomeHoushmand", DslFunctionRole.Income)]
    [InlineData("BazAfarine", DslFunctionRole.Income)]
    [InlineData("Tahator", DslFunctionRole.Tahator)]
    [InlineData("Tahator1", DslFunctionRole.Tahator)]
    [InlineData("iNcOMECheck", DslFunctionRole.IncomeCheck)]
    [InlineData("Run", DslFunctionRole.EntryPoint)]
    public void GetRole_classifies_member_functions(string name, DslFunctionRole expected)
    {
        Assert.Equal(expected, SupportedDslFunctions.GetRole(name));
        Assert.True(SupportedDslFunctions.IsSupported(name));
    }

    [Fact]
    public void AppliesToFiche_duty_skips_income_and_tahator()
    {
        var fiche = new FicheHeaderDto { Category = FicheCategory.DutyNosazi, DocTyp = 1 };
        Assert.True(SupportedDslFunctions.AppliesToFiche("Nosazi", "نوسازی", fiche));
        Assert.True(SupportedDslFunctions.AppliesToFiche("ChangeDate", null, fiche));
        Assert.False(SupportedDslFunctions.AppliesToFiche("iNcOME", "درآمد", fiche));
        Assert.False(SupportedDslFunctions.AppliesToFiche("Tahator", null, fiche));
    }

    [Fact]
    public void AppliesToFiche_income_includes_tahator_helpers()
    {
        var fiche = new FicheHeaderDto { Category = FicheCategory.Income, DocTyp = 12 };
        Assert.True(SupportedDslFunctions.AppliesToFiche("iNcOME", null, fiche));
        Assert.True(SupportedDslFunctions.AppliesToFiche("Tahator", null, fiche));
        Assert.True(SupportedDslFunctions.AppliesToFiche("ChangeDate", null, fiche));
        Assert.False(SupportedDslFunctions.AppliesToFiche("Nosazi", "نوسازی", fiche));
    }

    [Fact]
    public void RequiredRolesBeforeSoap_tahator_doc_requires_income_and_tahator()
    {
        var fiche = new FicheHeaderDto { Category = FicheCategory.Income, DocTyp = 14 };
        Assert.Equal(
            new[] { DslFunctionRole.Income, DslFunctionRole.Tahator },
            SupportedDslFunctions.RequiredRolesBeforeSoap(fiche));
    }

    [Fact]
    public void Execute_tahator_doc_applies_rules_before_soap_success()
    {
        var program = LoadParityProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            DocTyp = 15,
            FicheNo = "tahator-test",
            Payable = 100m,
            Rows = { new IncmRowDto { IncmNo = 1, Val = 100m } }
        };

        var result = executor.Execute(program, new DslExecutionContext { Fiche = fiche, DryRun = true });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Empty(result.PreSoapRuleErrors);
        Assert.Contains(result.AppliedFunctions, f => SupportedDslFunctions.IsIncome(f));
        Assert.Contains(result.AppliedFunctions, f => SupportedDslFunctions.IsTahator(f));
        Assert.Contains(result.Trace, t => t.Contains("PreSOAP OK: نقش Tahator", StringComparison.Ordinal));
    }

    [Fact]
    public void Execute_duty_does_not_mark_income_functions_unsupported()
    {
        var program = LoadParityProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            DocTyp = 1,
            FicheNo = "duty-test",
            Payable = 50m,
            Rows = { new IncmRowDto { IncmNo = 2003, Val = 50m } }
        };

        var result = executor.Execute(program, new DslExecutionContext { Fiche = fiche, DryRun = true });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal("Nosazi", result.DispatchedFunction, ignoreCase: true);
        Assert.DoesNotContain(result.Trace, t => t.Contains("Unsupported", StringComparison.OrdinalIgnoreCase)
            && t.Contains("بدنه", StringComparison.Ordinal));
        Assert.Contains(result.Trace, t => t.Contains("اعمال قانون role=Duty", StringComparison.Ordinal));
    }

    private static DslProgram LoadParityProgram()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-run-parity.xml");
        Assert.True(File.Exists(fixturePath), fixturePath);
        var xml = File.ReadAllText(fixturePath);
        return VbTranspiler.Transpile(XmlEnvelopeReader.Read(xml, "parity").Document);
    }
}

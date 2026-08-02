using Xunit;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Tests;

public class VbTranspilerTests
{
    [Fact]
    public void Parse_fixture_extracts_Run_and_Nosazi_dispatch()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-sample.xml");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        var xml = File.ReadAllText(fixturePath);
        var envelope = XmlEnvelopeReader.Read(xml, "fixture");
        var program = VbTranspiler.Transpile(envelope.Document);

        Assert.True(program.HasEntryPoint);
        Assert.True(program.HasNosazi);
        Assert.Contains(program.UnsupportedFunctions, f => f.Equals("iNcOME", StringComparison.OrdinalIgnoreCase));

        var run = program.Functions.First(f => f.Name.Equals("Run", StringComparison.OrdinalIgnoreCase));
        Assert.True(run.IsSupported);
        Assert.Contains(run.Body, s => s is DslIfStatement);

        var nosazi = program.Functions.First(f => f.Name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(nosazi.Body, s => s is DslAssignStatement a
            && a.Expression.Contains("GetAccountingDocCreateParameter", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateForPromotion_ignores_misparsed_assignment_artifacts()
    {
        var program = new DslProgram
        {
            EntryPoint = "Run",
            Functions = new List<DslFunction>
            {
                new()
                {
                    Name = "Run",
                    IsSupported = true,
                    Body = new List<DslStatement>
                    {
                        new DslCallOperationStatement("DutyFicheResultList", "DistrickBranch = Info8.GetAccountingDocCreateParameter", Array.Empty<string>()),
                        new DslCallOperationStatement("DutyFicheResultList", "Select Case Info8.GetAccountingDocCreateParameter", Array.Empty<string>()),
                        new DslCallOperationStatement("Save", "ClsAccounting", Array.Empty<string>())
                    }
                },
                new() { Name = "Nosazi", IsSupported = true }
            }
        };

        var validator = new DslValidator(SaraOperationBootstrap.CreateDefault());
        var result = validator.ValidateForPromotion(program);

        Assert.True(result.Success, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Parse_dotted_property_assign_is_assign_not_operation()
    {
        var body = """
            DtoAccounting_DocHeader.EumAccountingObjInDocument = CByte(Enums.AccountingObjectInDocument.DutyFiche)
            DistrickBranch = Info8.GetAccountingDocCreateParameter(param).DutyFicheResultList
            """;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nosazi" };
        var warnings = new List<string>();
        var statements = VbStatementParser.ParseBlock(body, names, warnings);

        Assert.All(statements, s => Assert.IsType<DslAssignStatement>(s));
    }

    [Fact]
    public void Parse_dim_as_new_list_is_assign_not_operation()
    {
        var body = """
            Dim ListRefP As New List(Of String)
            Dim PParamName As New List(Of String)
            Return ""
            """;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nosazi" };
        var warnings = new List<string>();
        var statements = VbStatementParser.ParseBlock(body, names, warnings);

        Assert.All(statements, s => Assert.IsNotType<DslCallOperationStatement>(s));
        Assert.Contains(statements, s => s is DslAssignStatement a && a.Target == "ListRefP");
    }
}

using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388ParityTests
{
    private static readonly string[] ExpectedIncomeCallOrder =
    {
        "iNcOME",
        "IncomeHoushmand",
        "IncomeSrvElectronic",
        "iNcOMESeprdeh",
        "iNcOMEEshghal",
        "iNcOMEGhatar_Shahri",
        "iNcOMEBackSeprdeh",
        "iNcOMEOragh",
        "iNcOMEHavaleT",
        "BazAfarine",
        "Tahator1",
        "Tahator"
    };

    [Fact]
    public void Transpile_marks_all_functions_supported_including_private()
    {
        var program = LoadParityProgram();

        Assert.Empty(program.UnsupportedFunctions);
        Assert.All(program.Functions, f => Assert.True(f.IsSupported, f.Name));
        Assert.Contains(program.Functions, f => f.Name.Equals("ChangeDate", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(program.Functions, f => f.Name.Equals("GetSara8Workflow", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(program.Functions, f => f.Name.Equals("GetDiffDate", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(program.Functions, f => f.Name.Equals("BedeHi", StringComparison.OrdinalIgnoreCase));
        Assert.True(program.Functions.Count >= 22);
    }

    [Fact]
    public void Execute_income_follows_same_Run_call_chain_as_xmlbody()
    {
        var program = LoadParityProgram();
        var registry = SaraOperationBootstrap.CreateDefault();
        var executor = new DslExecutor(registry);

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "050733453546",
            Payable = 1000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600m },
                new IncmRowDto { IncmNo = 1271, Val = 400m }
            }
        };

        var context = new DslExecutionContext { Fiche = fiche, DryRun = true };
        var result = executor.Execute(program, context);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(ExpectedIncomeCallOrder, context.InvokedFunctions);
        Assert.Equal(1000m, result.RowSum);
        Assert.Equal("iNcOME", result.DispatchedFunction, ignoreCase: true);
    }

    [Fact]
    public void Execute_duty_calls_Nosazi_like_xmlbody_else_branch()
    {
        var program = LoadParityProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "101104/9881711",
            Payable = 500m,
            Rows = { new IncmRowDto { IncmNo = 2003, Val = 500m } }
        };

        var context = new DslExecutionContext { Fiche = fiche, DryRun = true };
        var result = executor.Execute(program, context);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(new[] { "Nosazi" }, context.InvokedFunctions);
        Assert.Equal("Nosazi", result.DispatchedFunction, ignoreCase: true);
        Assert.Equal(500m, result.RowSum);
    }

    [Fact]
    public void Transpile_uploaded_member_1388_all_supported()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "RuleEngine", "Parser", "Fixtures", "member-1388-full.xml"));
        if (!File.Exists(path))
        {
            // fallback: cloud upload path used in older runs
            path = "/home/ubuntu/.cursor/projects/workspace/uploads/rayvax_c9f4.txt";
            if (!File.Exists(path))
                return;
            // raw VB — wrap minimally for reader
            var vb = File.ReadAllText(path);
            var wrapped =
                "<?xml version=\"1.0\"?><ClsFunction><NidClass>360</NidClass><NidFunction>1388</NidFunction>" +
                $"<Name>Run</Name><Body>{System.Security.SecurityElement.Escape(vb)}</Body></ClsFunction>";
            var programFromVb = VbTranspiler.Transpile(XmlEnvelopeReader.Read(wrapped, "upload").Document);
            Assert.Empty(programFromVb.UnsupportedFunctions);
            Assert.Contains(programFromVb.Functions, f => f.Name.Equals("Tahator", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(programFromVb.Functions, f => f.Name.Equals("Tahator1", StringComparison.OrdinalIgnoreCase));
            return;
        }

        var xml = File.ReadAllText(path);
        var program = VbTranspiler.Transpile(XmlEnvelopeReader.Read(xml, "full").Document);

        Assert.Empty(program.UnsupportedFunctions);
        Assert.All(program.Functions, f => Assert.True(f.IsSupported, f.Name));
        Assert.True(program.Functions.Count >= 22);
        Assert.Contains(program.Functions, f => f.Name.Equals("Tahator", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(program.Functions, f => f.Name.Equals("Tahator1", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(program.Functions, f => f.Name.Equals("ChangeDate", StringComparison.OrdinalIgnoreCase));

        var tahator = program.Functions.First(f => f.Name.Equals("Tahator", StringComparison.OrdinalIgnoreCase));
        var tahator1 = program.Functions.First(f => f.Name.Equals("Tahator1", StringComparison.OrdinalIgnoreCase));
        Assert.True(tahator.Body.Count > 10, "Tahator must not be empty stub");
        Assert.True(tahator1.Body.Count > 10, "Tahator1 must not be empty stub");
    }

    private static DslProgram LoadParityProgram()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-run-parity.xml");
        Assert.True(File.Exists(fixturePath), fixturePath);
        var xml = File.ReadAllText(fixturePath);
        return VbTranspiler.Transpile(XmlEnvelopeReader.Read(xml, "parity").Document);
    }
}

using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using Xunit;

namespace RayvarzResend.Tests;

public class DslExecutorTests
{
    [Fact]
    public void Validate_fixture_passes_for_known_operations()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-sample.xml");
        var xml = File.ReadAllText(fixturePath);
        var envelope = XmlEnvelopeReader.Read(xml, "fixture");
        var program = VbTranspiler.Transpile(envelope.Document);

        var registry = SaraOperationBootstrap.CreateDefault();
        var validator = new DslValidator(registry);
        var result = validator.Validate(program);

        Assert.True(result.Success, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Execute_fixture_dispatches_Nosazi_for_duty_fiche()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-sample.xml");
        var xml = File.ReadAllText(fixturePath);
        var program = VbTranspiler.Transpile(XmlEnvelopeReader.Read(xml, "fixture").Document);

        var registry = SaraOperationBootstrap.CreateDefault();
        var executor = new DslExecutor(registry);

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "101104/9881711",
            Payable = 1000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 2003, Val = 800m, IncmRowDsc = "نوسازی" },
                new IncmRowDto { IncmNo = 100002, Val = 100m, IncmRowDsc = "آتش نشانی" },
                new IncmRowDto { IncmNo = 100003, Val = 100m, IncmRowDsc = "پسماند" }
            }
        };

        var context = new DslExecutionContext { Fiche = fiche, DryRun = true };
        var result = executor.Execute(program, context);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal("Nosazi", result.DispatchedFunction, ignoreCase: true);
        Assert.Equal(1000m, result.RowSum);
        Assert.Equal(3, result.Rows.Count);
    }

    [Fact]
    public void Registry_treats_list_add_as_known_noop()
    {
        var registry = SaraOperationBootstrap.CreateDefault();
        Assert.True(registry.IsKnown("TmpAccounting_DocDetailsList.Add"));
        Assert.True(registry.IsKnown("ListRefP.add"));
        Assert.True(registry.IsKnown("ListAcc.Add"));

        var ctx = new DslExecutionContext { DryRun = true };
        Assert.Null(registry.Invoke("ListRefP.Add", ctx, Array.Empty<string>()));
    }

    [Fact]
    public void Registry_has_at_least_26_operations()
    {
        var registry = SaraOperationBootstrap.CreateDefault();
        Assert.True(registry.KnownOperationKeys.Count >= 26,
            $"Expected >=26 operations, got {registry.KnownOperationKeys.Count}");
    }
}

using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class IncomeRowScalerTests
{
    [Fact]
    public void ScaleToPayable_matches_soap_normalize_and_sums_to_payable()
    {
        // الگوی واقعی golden #5: جمع ناخالص ≠ Payable (تخفیف)
        var rows = new List<IncmRowDto>
        {
            new() { IncmNo = 100116, Val = 94400000m },
            new() { IncmNo = 1025, Val = 3783000000m },
            new() { IncmNo = 1271, Val = 1744000000m },
            new() { IncmNo = 1288, Val = 37760000m },
            new() { IncmNo = 1267, Val = 143851424m }
        };
        var payable = 5_379_066_000m;
        Assert.NotEqual(payable, rows.Sum(r => r.Val));

        IncomeRowScaler.ScaleToPayable(rows, payable);

        Assert.Equal(payable, rows.Sum(r => r.Val));
        Assert.All(rows, r => Assert.True(r.Val > 0));
    }

    [Fact]
    public void ScaleToPayable_is_noop_when_already_equal()
    {
        var rows = new List<IncmRowDto>
        {
            new() { IncmNo = 1, Val = 300m },
            new() { IncmNo = 2, Val = 200m }
        };
        IncomeRowScaler.ScaleToPayable(rows, 500m);
        Assert.Equal(300m, rows[0].Val);
        Assert.Equal(200m, rows[1].Val);
    }

    [Fact]
    public void BuildIncomeRows_scales_gross_rows_before_validate()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-sample.xml");
        var xml = File.ReadAllText(fixturePath);
        var program = VbTranspiler.Transpile(XmlEnvelopeReader.Read(xml, "fixture").Document);

        var registry = SaraOperationBootstrap.CreateDefault();
        var executor = new DslExecutor(registry);

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "050733453546",
            Payable = 1_000_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 800_000m, IncmRowDsc = "جریمه" },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m, IncmRowDsc = "زیربنا" }
            }
        };

        var result = executor.Execute(program, new DslExecutionContext { Fiche = fiche, DryRun = true });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(1_000_000m, result.RowSum);
        Assert.Equal(1_000_000m, result.Rows.Sum(r => r.Val));
    }
}

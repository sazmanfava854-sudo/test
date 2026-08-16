using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

/// <summary>
/// باگ ۱۰ — Income_Calculation (Sara) در برابر ray.incmdocsys.
/// نمونه واقعی: فیش شهرسازی 050733453546 (golden #5 از Phase6 seed / incmdocsys).
/// </summary>
public class IncomeCalculationIncmdocsysTests
{
    /// <summary>مقادیر ناخالص تقریبی dbo.Income_Calculation — از IncomeRowScalerTests شاخه parity.</summary>
    public static readonly IncmRowDto[] G5_GrossIncomeCalculation =
    [
        new() { IncmNo = 100116, Val = 94_400_000m, IncmRowDsc = "ماده 9" },
        new() { IncmNo = 1025, Val = 3_783_000_000m, IncmRowDsc = "ماده 100" },
        new() { IncmNo = 1271, Val = 1_744_000_000m, IncmRowDsc = "زیربنا" },
        new() { IncmNo = 1288, Val = 37_760_000m, IncmRowDsc = "آتش‌نشانی پایانکار" },
        new() { IncmNo = 1267, Val = 143_851_424m, IncmRowDsc = "مستحدثات" },
    ];

    /// <summary>مقادیر ثبت‌شده در ray.incmdocsys — فیش 050733453546 (منبع: 04_RuleGolden_Seed_Phase6_Samples.sql).</summary>
    public static readonly (int IncmNo, decimal Val)[] G5_IncmdocsysExpected =
    [
        (100116, 87_501_332m),
        (1025, 3_506_537_488m),
        (1271, 1_616_686_008m),
        (1288, 35_000_533m),
        (1267, 133_340_639m),
    ];

    public const string G5_FicheNo = "050733453546";
    public const decimal G5_Payable = 5_379_066_000m;

    [Fact]
    public void G5_gross_income_calculation_scales_to_payable_with_incmdocsys_incm_nos()
    {
        var grossSum = G5_GrossIncomeCalculation.Sum(r => r.Val);
        Assert.NotEqual(G5_Payable, grossSum);

        var rows = IncomeCalculationPipeline.PrepareRows(G5_GrossIncomeCalculation, G5_Payable);

        Assert.Equal(G5_IncmdocsysExpected.Length, rows.Count);
        Assert.Equal(G5_Payable, rows.Sum(r => r.Val));
        Assert.Equal(
            G5_IncmdocsysExpected.Select(e => e.IncmNo).OrderBy(x => x),
            rows.Select(r => r.IncmNo).OrderBy(x => x));

        // نسبت اسکیل یکنواخت (تخفیف سراسری) — همان منطق VB/NormalizeRows
        var factor = G5_Payable / grossSum;
        foreach (var row in rows)
        {
            var gross = G5_GrossIncomeCalculation.Single(g => g.IncmNo == row.IncmNo).Val;
            var expectedApprox = Math.Round(gross * factor, 0);
            Assert.InRange(row.Val, expectedApprox - 2, expectedApprox + 2);
        }
    }

    [Fact]
    public void G5_incmdocsys_expected_rows_match_authoritative_seed()
    {
        Assert.Equal(G5_Payable, G5_IncmdocsysExpected.Sum(e => e.Val));
        Assert.DoesNotContain(G5_IncmdocsysExpected, e => e.IncmNo == 100202);
    }

    [Fact]
    public void Excluded_100202_not_in_pipeline_and_does_not_affect_scaling()
    {
        var raw = G5_GrossIncomeCalculation
            .Append(new IncmRowDto { IncmNo = 100202, Val = 999_999_999m, IncmRowDsc = "متا" })
            .ToArray();

        var rows = IncomeCalculationPipeline.PrepareRows(raw, G5_Payable);

        Assert.DoesNotContain(rows, r => r.IncmNo == 100202);
        Assert.Equal(G5_Payable, rows.Sum(r => r.Val));
        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public void IncomeExcludedCodes_includes_100202_like_member1388()
    {
        Assert.Contains(100202, IncomeExcludedCodes.Codes);
    }

    [Theory]
    [MemberData(nameof(ShahrsaziIncmdocsysSamples))]
    public void Shahrsazi_incmdocsys_rows_sum_to_payable(
        string ficheNo, decimal payable, (int, decimal)[] expectedRows)
    {
        _ = ficheNo;

        var rows = expectedRows
            .Select(e => new IncmRowDto { IncmNo = e.Item1, Val = e.Item2 })
            .ToList();

        IncomeRowScaler.ScaleToPayable(rows, payable);

        Assert.Equal(payable, rows.Sum(r => r.Val));
        Assert.Equal(expectedRows.Length, rows.Count);
        foreach (var (incmNo, val) in expectedRows)
            Assert.Equal(val, rows.Single(r => r.IncmNo == incmNo).Val);
    }

    public static TheoryData<string, decimal, (int, decimal)[]> ShahrsaziIncmdocsysSamples =>
        new()
        {
            { G5_FicheNo, G5_Payable, G5_IncmdocsysExpected },
            {
                "050733451977", 2_024_365_000m,
                new[]
                {
                    (100116, 93_720_602m),
                    (1270, 1_874_412_037m),
                    (1278, 56_232_361m)
                }
            },
            {
                "050733447710", 1_780_716_000m,
                new[]
                {
                    (1239, 983_814_237m),
                    (1270, 736_951_109m),
                    (1272, 920_894m),
                    (100116, 36_893_600m),
                    (1278, 22_136_160m)
                }
            },
            {
                "050733454216", 147_291_000m,
                new[]
                {
                    (100116, 6_563_894m),
                    (1025, 6_823_650m),
                    (1271, 131_277_898m),
                    (1288, 2_625_558m)
                }
            }
        };

    [Fact]
    public void Soap_uses_pipeline_rows_without_double_scaling_for_incmdocsys_G5()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = G5_FicheNo,
            Payable = G5_Payable,
            BillId = "9000000000000",
            PaymentId = "0000000000000",
            BnkAcntNo = "7-8-72-47-1-0-2",
            DocTyp = 3,
            Rows = G5_IncmdocsysExpected
                .Select(e => new IncmRowDto { IncmNo = e.IncmNo, Val = e.Val })
                .ToList()
        };

        var xml = BuildIncomeSoap(fiche, branch: 207, fund: 200207009);

        foreach (var (incmNo, val) in G5_IncmdocsysExpected)
        {
            var qty = val.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains($"<b:IncmNo>{incmNo}</b:IncmNo>", xml);
            Assert.Contains($"<b:Val>{qty}</b:Val>", xml);
        }

        Assert.DoesNotContain("<b:IncmNo>100202</b:IncmNo>", xml);
    }

    [Fact]
    public void IncomeRowScaler_matches_soap_normalize_for_gross_G5()
    {
        var pipelineRows = IncomeCalculationPipeline.PrepareRows(G5_GrossIncomeCalculation, G5_Payable);

        var soapRows = G5_GrossIncomeCalculation
            .Where(r => !IncomeExcludedCodes.Codes.Contains(r.IncmNo) && r.Val != 0)
            .Select(r => new IncmRowDto { IncmNo = r.IncmNo, Val = r.Val })
            .ToList();

        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            Payable = G5_Payable,
            Rows = soapRows
        };

        var xml = BuildIncomeSoap(fiche, 207, 200207009);
        // NormalizeRows داخل Build اجرا می‌شود — مقادیر Val در XML باید با pipeline یکی باشد
        foreach (var row in pipelineRows)
        {
            var val = row.Val.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains($"<b:Val>{val}</b:Val>", xml);
        }
    }

    private static string BuildIncomeSoap(FicheHeaderDto fiche, int branch, int fund)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
                ["Rayvarz:ServiceUrl"] = "http://example.local/svc",
                ["Rayvarz:RefRowDocNoInDetail"] = "zero",
                ["Rayvarz:IncmMkrTyp"] = "auto"
            })
            .Build();
        return new SoapBuilder(config).Build(fiche, branch, fund, null, null, null);
    }
}

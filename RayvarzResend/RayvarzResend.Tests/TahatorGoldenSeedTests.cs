using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

/// <summary>
/// Regression guard for <c>database/06_RuleGolden_Seed_Tahator.sql</c> — golden IDs 11–14 (مسیر ۱۵۷ / مبلغ).
/// </summary>
public class TahatorGoldenSeedTests
{
    private static string GoldenSeedSqlPath => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "database", "06_RuleGolden_Seed_Tahator.sql"));

    private static string TahatorSetupDocPath => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "TAHATOR-SETUP.md"));

    public static TheoryData<int, string, decimal, long, long> AmountGoldenSamples => new()
    {
        { 11, "050933483716", -22_106_681_457m, 320008535, 700100001 },
        { 12, "051133444502", -5_676_696_274m, 320008535, 700100001 },
        { 13, "051133450714", -3_603_899_024m, 320008535, 700100001 },
        { 14, "051233468141", -26_841_652_707m, 320008535, 700100001 },
    };

    [Fact]
    public void Golden_seed_sql_exists_and_targets_ids_11_through_14()
    {
        Assert.True(File.Exists(GoldenSeedSqlPath), GoldenSeedSqlPath);
        var sql = File.ReadAllText(GoldenSeedSqlPath);
        Assert.Contains("RuleGoldenFiche", sql);
        Assert.Contains("RuleGoldenExpectedRow", sql);
        Assert.Contains("ExpectedCenter1", sql);
        Assert.Contains("DELETE FROM dbo.RuleGoldenExpectedRow WHERE GoldenFicheId BETWEEN 11 AND 14", sql);
        foreach (var sample in AmountGoldenSamples)
        {
            var id = (int)sample[0]!;
            var ficheNo = (string)sample[1]!;
            Assert.Contains($"({id},", sql);
            Assert.Contains(ficheNo, sql);
        }
    }

    [Theory]
    [MemberData(nameof(AmountGoldenSamples))]
    public void Golden_seed_sql_contains_incmdocsys_expected_row(
        int goldenId, string ficheNo, decimal expectedVal, long center1, long center3)
    {
        var sql = File.ReadAllText(GoldenSeedSqlPath);
        Assert.Contains(ficheNo, sql);
        var valToken = expectedVal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Contains(valToken, sql);
        Assert.Contains($"({goldenId}, 1, 200098", sql);
        Assert.Contains(center1.ToString(), sql);
        Assert.Contains(center3.ToString(), sql);
    }

    [Theory]
    [MemberData(nameof(AmountGoldenSamples))]
    public void ApplyTahatorRows_matches_golden_amount_sample(
        int goldenId, string ficheNo, decimal expectedVal, long center1, long center3)
    {
        Assert.InRange(goldenId, 11, 14);
        Assert.False(string.IsNullOrEmpty(ficheNo));
        var payable = Math.Abs(expectedVal);
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            BankCode = "4",
            CheckNo = "6",
            Deposit = center1,
            Payable = payable,
            Rows = { new IncmRowDto { IncmNo = 999, Val = 1 } }
        };

        TahatorRowBuilder.ApplyTahatorRows(fiche);

        Assert.Equal(14, fiche.DocTyp);
        Assert.Single(fiche.Rows);
        Assert.Equal(200098, fiche.Rows[0].IncmNo);
        Assert.Equal(expectedVal, fiche.Rows[0].Val);
        Assert.Equal(center1, fiche.Rows[0].Center1);
        Assert.Equal(center3, fiche.Rows[0].Center3);
        Assert.Equal(102, TahatorRowBuilder.DefaultRayvarzBranch);
    }

    [Fact]
    public void TAHATOR_SETUP_doc_lists_same_golden_amount_table()
    {
        Assert.True(File.Exists(TahatorSetupDocPath), TahatorSetupDocPath);
        var doc = File.ReadAllText(TahatorSetupDocPath);
        Assert.Contains("06_RuleGolden_Seed_Tahator.sql", doc);
        Assert.Contains("050933483716", doc);
        Assert.Contains("051233468141", doc);
        Assert.Contains("−22,106,681,457", doc);
        Assert.Contains("۴ فیش گلدن فعلی فقط مسیر **۱۵۷ / مبلغ**", doc);
    }

    [Fact]
    public void Golden_income_158_samples_reserved_above_id_14()
    {
        var sql = File.ReadAllText(GoldenSeedSqlPath);
        Assert.DoesNotContain("GoldenFicheId BETWEEN 15", sql);
        Assert.DoesNotContain("(15,", sql);
    }
}

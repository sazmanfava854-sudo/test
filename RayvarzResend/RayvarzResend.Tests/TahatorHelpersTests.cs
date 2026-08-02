using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class TahatorHelpersTests
{
    [Fact]
    public void CurrentShamsiSlashDate_has_yyyy_MM_dd_shape()
    {
        var s = DateHelper.CurrentShamsiSlashDate();
        Assert.Matches(@"^\d{4}/\d{2}/\d{2}$", s);
    }

    [Theory]
    [InlineData("14050323", "1405/03/23")]
    [InlineData("1405/03/23", "1405/03/23")]
    [InlineData("", "")]
    public void ToShamsiSlashDate_normalizes(string input, string expected)
    {
        Assert.Equal(expected, DateHelper.ToShamsiSlashDate(input));
    }

    [Theory]
    [InlineData("4", 14)]
    [InlineData("18", 15)]
    [InlineData("", 15)]
    public void ApplyTahatorDocTyp_matches_member_Tahator1(string bank, int expectedDocTyp)
    {
        var fiche = new FicheHeaderDto { BankCode = bank, DocTyp = 3 };
        TahatorResendService.ApplyTahatorDocTyp(fiche);
        Assert.Equal(expectedDocTyp, fiche.DocTyp);
        Assert.Equal("تهاتر مبلغ", fiche.DocTypDsc);
        Assert.Equal("اسناد تهاتر مبلغ", fiche.DocDsc);
    }

    [Fact]
    public void Schema_script_defines_TahatorRestoreSnapshot()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "database", "05_TahatorRestoreSnapshot.sql"));
        Assert.True(File.Exists(path), path);
        var sql = File.ReadAllText(path);
        Assert.Contains("TahatorRestoreSnapshot", sql);
        Assert.Contains("ExportPermanentDate", sql);
        Assert.Contains("Pending", sql);
    }

    [Fact]
    public void ApplyTahatorRows_matches_sample_incmdocsys_centers()
    {
        // الگوی نمونه‌های golden 11–14: CI_Bank=4, CheckNo=6, Deposit=320008535
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            BankCode = "4",
            CheckNo = "6",
            Deposit = 320008535,
            DepositId = 19684,
            CreditorPapers = 5510918,
            Payable = 22_106_681_457m,
            Rows = { new IncmRowDto { IncmNo = 999, Val = 1 } }
        };

        TahatorRowBuilder.ApplyTahatorRows(fiche);

        Assert.Equal(14, fiche.DocTyp);
        Assert.Equal(0, fiche.Center);
        Assert.Single(fiche.Rows);
        var row = fiche.Rows[0];
        Assert.Equal(200098, row.IncmNo);
        Assert.Equal(-22_106_681_457m, row.Val);
        Assert.Equal("مبلغ تهاتر", row.IncmRowDsc);
        Assert.Equal(320008535, row.Center1);
        Assert.Null(row.Center2);
        Assert.Equal(700100001, row.Center3);
        Assert.Equal("19684", row.Ref);
        Assert.Equal("4", row.Num);
        Assert.True(TahatorRowBuilder.RowSumMatchesPayable(fiche, row.Val));
    }

    [Fact]
    public void ApplyTahatorRows_CheckNo5_uses_Center3_700100002()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            BankCode = "4",
            CheckNo = "5",
            Deposit = 1,
            Payable = 100m
        };
        TahatorRowBuilder.ApplyTahatorRows(fiche);
        Assert.Equal(700100002, fiche.Rows[0].Center3);
    }

    [Fact]
    public void Golden_seed_script_includes_tahator_samples_and_centers()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "database", "06_RuleGolden_Seed_Tahator.sql"));
        Assert.True(File.Exists(path), path);
        var sql = File.ReadAllText(path);
        Assert.Contains("050933483716", sql);
        Assert.Contains("051133444502", sql);
        Assert.Contains("051133450714", sql);
        Assert.Contains("051233468141", sql);
        Assert.Contains("ExpectedCenter1", sql);
        Assert.Contains("ExpectedCenter3", sql);
        Assert.Contains("200098", sql);
        Assert.Contains("320008535", sql);
        Assert.Contains("700100001", sql);
    }
}

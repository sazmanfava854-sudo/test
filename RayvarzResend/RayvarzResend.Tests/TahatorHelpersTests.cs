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
        // الگوی نمونه‌های golden 11–14 (مقادیر expected)؛ منطق از Tahator1 XmlBody است
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
        Assert.Equal(0, fiche.Center); // CI_Bank≠2 → "0" در Tahator1
        Assert.Single(fiche.Rows);
        var row = fiche.Rows[0];
        Assert.Equal(200098, row.IncmNo);
        Assert.Equal(-22_106_681_457m, row.Val);
        Assert.Equal("مبلغ تهاتر", row.IncmRowDsc);
        Assert.Equal(320008535, row.Center1); // Tahator1: deposit
        Assert.Null(row.Center2); // Tahator1 Center2 را ست نمی‌کند
        Assert.Equal(700100001, row.Center3); // CheckNo≠5
        Assert.Equal("19684", row.Ref);
        Assert.Equal("4", row.Num);
        Assert.True(TahatorRowBuilder.RowSumMatchesPayable(fiche, row.Val));
    }

    [Fact]
    public void ApplyTahatorRows_Bank2_Center_uses_CreditorPapers_per_Tahator1()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            BankCode = "2",
            CreditorPapers = 5510918,
            Deposit = 10,
            CheckNo = "6",
            Payable = 100m
        };
        TahatorRowBuilder.ApplyTahatorRows(fiche);
        Assert.Equal(5510918, fiche.Center);
        Assert.Equal(200099, fiche.Rows[0].IncmNo); // CI_Bank≠4
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

    [Theory]
    [InlineData("9-3-161-2-1-0-0", 209, 59)]
    [InlineData("11-4-125-15-1-0-0", 211, 61)]
    [InlineData("12-12-14-5-1-0-0", 212, 62)]
    public void Tahator1_fund_map_from_nosazi_nick(string nick, int district, int fund)
    {
        Assert.Equal(district, TahatorRowBuilder.ResolveDistrictBranchFromNosaziCode(nick));
        Assert.Equal(fund, TahatorRowBuilder.ResolveTahatorFund(district));
    }

    [Fact]
    public void NormalizeFicheNo_keeps_slash_as_stored_in_Sara()
    {
        Assert.Equal("040933/318150", TahatorRowBuilder.NormalizeFicheNo("040933/318150"));
        Assert.Equal("040933318150", TahatorRowBuilder.NormalizeFicheNo("040933318150"));
    }

    [Fact]
    public void FicheNoLookupVariants_tries_with_and_without_slash()
    {
        var fromSlash = TahatorRowBuilder.FicheNoLookupVariants("040933/318150");
        Assert.Contains("040933/318150", fromSlash);
        Assert.Contains("040933318150", fromSlash);

        var fromPlain = TahatorRowBuilder.FicheNoLookupVariants("040933318150");
        Assert.Contains("040933318150", fromPlain);
        Assert.Contains("040933/318150", fromPlain);
    }

    [Fact]
    public void ApplyTahatorRows_preserves_slash_in_FicheNo()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            BankCode = "4",
            CheckNo = "6",
            Deposit = 310134334,
            DepositId = 10987476,
            Payable = 2_458_668_372m,
            BnkAcntNo = "9-3-161-2-1-0-0",
            FicheNo = "040933/318150"
        };
        TahatorRowBuilder.ApplyTahatorRows(fiche);
        Assert.Equal("040933/318150", fiche.FicheNo);
        Assert.Equal(209, fiche.ResolvedDistrictBranch);
        Assert.Equal(59, fiche.SuggestedFund);
        Assert.Equal(14, fiche.DocTyp);
        Assert.Equal(200098, fiche.Rows[0].IncmNo);
        Assert.Equal(310134334, fiche.Rows[0].Center1);
    }

    [Fact]
    public void Golden_seed_script_includes_tahator_samples_and_centers()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "database", "06_RuleGolden_Seed_Tahator.sql"));
        Assert.True(File.Exists(path), path);
        var sql = File.ReadAllText(path);
        Assert.Contains("050933483716", sql);
        Assert.Contains("ExpectedCenter1", sql);
        Assert.Contains("200098", sql);
    }
}

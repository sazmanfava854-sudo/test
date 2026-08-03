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
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            BankCode = bank,
            DocTyp = 3
        };
        TahatorResendService.ApplyTahatorDocTyp(fiche);
        Assert.Equal(expectedDocTyp, fiche.DocTyp);
        Assert.Equal("تهاتر مبلغ", fiche.DocTypDsc);
        Assert.Equal("اسناد تهاتر مبلغ", fiche.DocDsc);
    }

    [Theory]
    [InlineData("4", 17)]
    [InlineData("18", 18)]
    [InlineData("2", 18)]
    [InlineData("", 18)]
    public void ApplyTahatorDocTyp_matches_Tahator_income_17_18(string bank, int expectedDocTyp)
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            BankCode = bank,
            DocTyp = 3
        };
        TahatorResendService.ApplyTahatorDocTyp(fiche);
        Assert.Equal(expectedDocTyp, fiche.DocTyp);
        Assert.Equal("تهاتر درآمد", fiche.DocTypDsc);
        Assert.Equal("اسناد تهاتر درآمد", fiche.DocDsc);
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

    [Theory]
    [InlineData("9-3-161-2-1-0-0", 209, 39)]
    [InlineData("11-4-125-15-1-0-0", 211, 41)]
    [InlineData("12-12-14-5-1-0-0", 212, 42)]
    [InlineData("1-2-3-0-0-0-0", 201, 31)]
    public void Tahator_income_fund_map_31_42(string nick, int district, int fund)
    {
        Assert.Equal(district, TahatorRowBuilder.ResolveDistrictBranchFromNosaziCode(nick));
        Assert.Equal(fund, TahatorRowBuilder.ResolveTahatorIncomeFund(district));
    }

    [Fact]
    public void ApplyTahatorIncomeRows_DocTyp17_positive_Val_Center1_fixed_region_fund()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            BankCode = "4",
            Payable = 1_500_000m,
            CreditorPapers = 5510918,
            BnkAcntNo = "9-3-161-2-1-0-0",
            Rows =
            {
                new IncmRowDto { IncmNo = 1201, Val = 1_000_000m },
                new IncmRowDto { IncmNo = 1202, Val = 500_000m }
            }
        };

        TahatorRowBuilder.ApplyTahatorRows(fiche);

        Assert.True(TahatorRowBuilder.IsTahatorIncomeFiche(fiche));
        Assert.False(TahatorRowBuilder.IsTahatorAmountFiche(fiche));
        Assert.Equal(17, fiche.DocTyp);
        Assert.Equal(0, fiche.Center); // Bank≠2
        Assert.Equal(209, fiche.ResolvedDistrictBranch);
        Assert.Equal(39, fiche.SuggestedFund); // منطقه → Fund ۳۱–۴۲
        Assert.Equal(2, fiche.Rows.Count);
        Assert.All(fiche.Rows, r =>
        {
            Assert.Equal(TahatorRowBuilder.TahatorIncomeCenter1, r.Center1);
            Assert.True(r.Val > 0);
        });
        Assert.True(TahatorRowBuilder.RowSumMatchesPayable(fiche, fiche.Rows.Sum(r => r.Val)));
    }

    [Fact]
    public void ApplyTahatorIncomeRows_Bank2_DocTyp18_Center_CreditorPapers()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            BankCode = "2",
            CreditorPapers = 5510918,
            Payable = 100m,
            BnkAcntNo = "5-1-1-0-0-0-0"
        };
        TahatorRowBuilder.ApplyTahatorRows(fiche);
        Assert.Equal(18, fiche.DocTyp);
        Assert.Equal(5510918, fiche.Center);
        Assert.Equal(205, fiche.ResolvedDistrictBranch);
        Assert.Equal(35, fiche.SuggestedFund);
        Assert.Equal(TahatorRowBuilder.TahatorIncomeCenter1, fiche.Rows[0].Center1);
        Assert.Equal(100m, fiche.Rows[0].Val);
    }

    [Fact]
    public void Group_157_wins_over_stale_DocTyp_17()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            DocTyp = 17,
            BankCode = "4",
            Deposit = 1,
            Payable = 50m
        };
        Assert.True(TahatorRowBuilder.IsTahatorAmountFiche(fiche));
        Assert.False(TahatorRowBuilder.IsTahatorIncomeFiche(fiche));
        TahatorRowBuilder.ApplyTahatorRows(fiche);
        Assert.Equal(14, fiche.DocTyp);
        Assert.Equal(-50m, fiche.Rows[0].Val);
    }

    [Fact]
    public void NormalizeFicheNo_keeps_slash_exact_no_variant()
    {
        Assert.Equal("040933/318150", TahatorRowBuilder.NormalizeFicheNo("040933/318150"));
        Assert.Equal("040933318150", TahatorRowBuilder.NormalizeFicheNo("040933318150"));
        Assert.NotEqual(
            TahatorRowBuilder.NormalizeFicheNo("040933/318150"),
            TahatorRowBuilder.NormalizeFicheNo("040933318150"));
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

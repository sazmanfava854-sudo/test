using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

/// <summary>باگ‌های بحرانی v21 — Fund UI، تاریخ UI، تهاتر pair abort، branch 158.</summary>
public class CriticalFixesTests
{
    private static IConfiguration TestConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
                ["Rayvarz:SourceSystemId"] = "TEST",
            })
            .Build();

    [Fact]
    public void SoapBuilder_request_fund_wins_over_SuggestedFund()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "050833446542",
            Payable = 1_000_000m,
            PaymentBranch = "18",
            SuggestedFund = 200209004,
            Rows = { new IncmRowDto { IncmNo = 1278, Val = 1_000_000m } }
        };

        var xml = new SoapBuilder(TestConfig()).Build(fiche, 209, fund: 200209008, "14050101", null, null);
        Assert.Contains("<b:Fund>200209008</b:Fund>", xml);
        Assert.DoesNotContain("<b:Fund>200209004</b:Fund>", xml);
    }

    [Fact]
    public void SoapBuilder_request_dates_win_over_loaded_fiche_dates()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "050833446542",
            Payable = 100m,
            RayvarzDocDate = "14040101",
            RayvarzActDate = "14040102",
            RayvarzDueDate = "14040103",
            Rows = { new IncmRowDto { IncmNo = 1278, Val = 100m } }
        };

        var xml = new SoapBuilder(TestConfig()).Build(
            fiche, 207, 200207009, docDate: "14051201", actDate: "14051202", dueDate: "14051203");

        Assert.Contains("<b:DocDate>14051201</b:DocDate>", xml);
        Assert.Contains("<b:ActDate>14051202</b:ActDate>", xml);
        Assert.Contains("<b:Due>14051203</b:Due>", xml);
        Assert.DoesNotContain("<b:DocDate>14040101</b:DocDate>", xml);
    }

    [Fact]
    public void ResolveSendBranch_income_158_uses_district_not_center()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            ResolvedDistrictBranch = 209,
            FicheNo = "050133472496"
        };
        Assert.Equal(209, TahatorRowBuilder.ResolveSendBranch(fiche, 0));
    }

    [Fact]
    public void ResolveSendBranch_income_158_without_district_throws()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            FicheNo = "050133472496"
        };
        var ex = Assert.Throws<InvalidOperationException>(() => TahatorRowBuilder.ResolveSendBranch(fiche, 0));
        Assert.Contains("158", ex.Message);
    }

    [Fact]
    public void ResolveSendBranch_amount_157_uses_center_102()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 157,
            FicheNo = "050133472495"
        };
        Assert.Equal(102, TahatorRowBuilder.ResolveSendBranch(fiche, 0));
    }
}

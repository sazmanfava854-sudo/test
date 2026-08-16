using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class FundResolverTests
{
  private static IConfiguration EmptyConfig() =>
    new ConfigurationBuilder().Build();

  [Theory]
  [InlineData(212, "18", 212210016)]
  [InlineData(212, "1", 200212004)]
  [InlineData(209, "18", 200209008)]
  [InlineData(209, "1", 200209004)]
  [InlineData(218, "18", 200218011)]
  [InlineData(218, "1", 1200)]
  public void Known_branch_uses_DutyDistrictBranchResolver(int branch, string bank, int expected)
  {
    var fund = FundResolver.Resolve(EmptyConfig(), branch, bank);
    Assert.Equal(expected, fund);
  }

  [Fact]
  public void Branch_212_bank18_matches_Program_cs_and_SuggestedFund()
  {
    var fund = FundResolver.Resolve(EmptyConfig(), 212, "18");
    Assert.Equal(DutyDistrictBranchResolver.ResolveFund(212, "18"), fund);
    Assert.Equal(212210016, fund);
  }

  [Fact]
  public void Unknown_branch_falls_back_to_FundMap()
  {
    var config = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?> { ["FundMap:999"] = "999999999" })
      .Build();
    Assert.Equal(999999999, FundResolver.Resolve(config, 999, "18"));
  }

  [Fact]
  public void SoapBuilder_uses_correct_fund_for_branch_212_when_fund_zero()
  {
    var fiche = new RayvarzResend.Web.Models.FicheHeaderDto
    {
      Category = RayvarzResend.Web.Models.FicheCategory.Income,
      FicheNo = "050833446542",
      Payable = 987_973_000m,
      PaymentBranch = "18",
      BankCode = "18",
      BnkAcntNo = "8-3-22-24-1-0-1",
      DocTyp = 3,
      DocDsc = "اسناد شهرسازی",
      BillId = "9000080251869",
      PaymentId = "0098797332591",
      RayvarzDocDate = "14050422",
      RayvarzActDate = "14050422",
      RayvarzDueDate = "14050422",
      Rows = { new RayvarzResend.Web.Models.IncmRowDto { IncmNo = 1262, Val = 987_973_000m, IncmRowDsc = "عوارض بر مشاغل" } }
    };

    var config = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
        ["Rayvarz:ServiceUrl"] = "http://example.local/svc",
        ["Rayvarz:RefRowDocNoInDetail"] = "zero"
      })
      .Build();

    var xml = new SoapBuilder(config).Build(fiche, 212, fund: 0, null, null, null);
    Assert.Contains("<b:Fund>212210016</b:Fund>", xml);
    Assert.DoesNotContain("<b:Fund>200212004</b:Fund>", xml);
  }
}

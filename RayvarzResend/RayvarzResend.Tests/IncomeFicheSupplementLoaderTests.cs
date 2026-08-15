using RayvarzResend.Web.RuleEngine.Executor;
using Xunit;

namespace RayvarzResend.Tests;

public class IncomeFicheSupplementLoaderTests
{
  [Theory]
  [InlineData(218, 3)]
  [InlineData(80, 3)]
  [InlineData(209, 35)]
  public void BedeHi_allowed_account_groups_match_vb_district_rules(int district, int expectedCount)
  {
    Assert.Equal(expectedCount, BedeHiLogic.AllowedAccountGroups(district).Count);
  }

  [Fact]
  public void BedeHi_regional_groups_are_1_7_10()
  {
    Assert.Equal(new[] { 1, 7, 10 }, BedeHiLogic.AllowedAccountGroups(218));
  }

  [Fact]
  public void BedeHi_standard_groups_include_common_income_groups()
  {
    var groups = BedeHiLogic.AllowedAccountGroups(209);
    Assert.Contains(8, groups);
    Assert.Contains(15, groups);
    Assert.Contains(64, groups);
  }
}

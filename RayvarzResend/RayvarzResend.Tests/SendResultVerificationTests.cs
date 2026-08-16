using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class SendResultVerificationTests
{
  [Theory]
  [InlineData(true, false, false, "SOAP موفق بود ولی فیش در incmdocsys تأیید نشد")]
  [InlineData(true, true, false, null)]
  [InlineData(false, false, false, null)]
  [InlineData(true, false, true, null)]
  public void BuildUnverifiedWarning_returns_expected(bool soapSuccess, bool verified, bool dryRun, string? expected)
  {
    Assert.Equal(expected, SendResultVerification.BuildUnverifiedWarning(soapSuccess, verified, dryRun));
  }
}

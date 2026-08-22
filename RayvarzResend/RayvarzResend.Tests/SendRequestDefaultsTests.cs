using RayvarzResend.Web.Models;
using Xunit;

namespace RayvarzResend.Tests;

public class SendRequestDefaultsTests
{
  [Fact]
  public void SendFicheRequest_has_no_ResetStatus_property()
  {
    var prop = typeof(SendFicheRequest).GetProperty("ResetStatus");
    Assert.Null(prop);
  }

  [Fact]
  public void UnsentBatchSendRequest_has_no_ResetStatus_property()
  {
    var prop = typeof(UnsentBatchSendRequest).GetProperty("ResetStatus");
    Assert.Null(prop);
  }
}

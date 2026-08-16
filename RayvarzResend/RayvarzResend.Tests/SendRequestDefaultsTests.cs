using System.Text.Json;
using System.Text.Json.Serialization;
using RayvarzResend.Web.Models;
using Xunit;

namespace RayvarzResend.Tests;

public class SendRequestDefaultsTests
{
  private static readonly JsonSerializerOptions JsonOpts = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };

  [Fact]
  public void SendFicheRequest_ResetStatus_defaults_to_false()
  {
    var req = new SendFicheRequest();
    Assert.False(req.ResetStatus);
  }

  [Fact]
  public void SendFicheRequest_deserialize_without_resetStatus_is_false()
  {
    var json = """{"branch":203,"fund":200203013}""";
    var req = JsonSerializer.Deserialize<SendFicheRequest>(json, JsonOpts);
    Assert.NotNull(req);
    Assert.False(req.ResetStatus);
  }

  [Fact]
  public void SendFicheRequest_deserialize_explicit_true_honored()
  {
    var json = """{"resetStatus":true}""";
    var req = JsonSerializer.Deserialize<SendFicheRequest>(json, JsonOpts);
    Assert.NotNull(req);
    Assert.True(req.ResetStatus);
  }
}

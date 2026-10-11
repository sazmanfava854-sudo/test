using System.Text.Json;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class MashhadSsoJsonTests
{
    [Fact]
    public void LoginKey_request_serializes_rule_engine_property_names()
    {
        var json = JsonSerializer.Serialize(new MashhadLoginKeyRequest
        {
            Time = "1",
            Hash = "abc",
            ClientId = "19cf3C33",
            State = "test",
            UserType = 0,
            DomainId = 0
        }, MashhadSsoJson.SerializerOptions);

        Assert.Contains("\"ClientId\":\"19cf3C33\"", json.Replace(" ", ""));
        Assert.Contains("\"DomainID\":0", json.Replace(" ", ""));
        Assert.Contains("\"UserType\":0", json.Replace(" ", ""));
    }

    [Fact]
    public void LoginKey_response_deserializes_loginKey_field()
    {
        var json = """{"ErrorCode":0,"ErrorMessage":"","Data":{"loginKey":"lk-123"}}""";
        var r = JsonSerializer.Deserialize<MashhadSsoResult<MashhadLoginKeyData>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(r);
        Assert.Equal(0, r!.ErrorCode);
        Assert.Equal("lk-123", r.Data?.EffectiveLoginKey);
    }
}

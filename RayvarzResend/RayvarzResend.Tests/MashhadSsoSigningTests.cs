using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class MashhadSsoSigningTests
{
    [Fact]
    public void CreateMaterial_uses_sha256_time_plus_secret_rule_engine()
    {
        var material = MashhadSsoSigning.CreateMaterial("myApiUser", "secretKey", "1700000000", "lower");
        Assert.Equal("myApiUser", material.ApiName);
        Assert.Equal("1700000000", material.RequestTime);
        Assert.Equal(SsoApiSecretHash.ComputeApiSecret("secretKey", "1700000000", "lower"), material.ApiSecret);
    }

    [Theory]
    [InlineData(401, "requestTime expired", true)]
    [InlineData(403, "Client info missmatched", false)]
    [InlineData(500, "SecretKey invalid or expired", true)]
    [InlineData(0, "", false)]
    public void ShouldRetryWithFreshRequestTime(int code, string msg, bool expected)
    {
        Assert.Equal(expected, MashhadSsoSigning.ShouldRetryWithFreshRequestTime(code, msg));
    }
}

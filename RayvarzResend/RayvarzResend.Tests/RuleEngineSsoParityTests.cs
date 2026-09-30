using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

/// <summary>هم‌ترازی با RuleEngineDocuments.Api.SSO (فایل SSO / loginKey).</summary>
public class RuleEngineSsoParityTests
{
    [Fact]
    public void ApiSecret_matches_RuleEngine_SSOSecret_plus_requestTime()
    {
        const string secret = "e9b501D2fbfe512722b5FB38a92a49F4";
        const string time = "1790765181";
        var ruleEngineRaw = secret + time;
        var expected = SsoApiSecretHash.Sha256AsciiHexLower(ruleEngineRaw);
        var fromApp = SsoApiSecretHash.ComputeApiSecret(secret, time, "lower", "SecretTime");
        Assert.Equal(expected, fromApp);
    }
}

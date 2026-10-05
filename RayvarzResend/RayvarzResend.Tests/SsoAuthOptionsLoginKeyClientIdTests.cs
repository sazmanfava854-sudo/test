using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class SsoAuthOptionsLoginKeyClientIdTests
{
    [Fact]
    public void EffectiveLoginKeyBodyClientId_uses_guid_when_not_forced_to_api_name()
    {
        var o = new SsoAuthOptions
        {
            ApiName = "FinancialAssistant",
            ClientId = "53db42619cf3C333b13a18D34fbd9111",
            LoginKeyBodyClientIdIsApiName = false
        };
        Assert.Equal("53db42619cf3C333b13a18D34fbd9111", o.EffectiveLoginKeyBodyClientId);
    }

    [Fact]
    public void EffectiveLoginKeyBodyClientId_uses_apiName_when_flag_enabled()
    {
        var o = new SsoAuthOptions
        {
            ApiName = "FinancialAssistant",
            ClientId = "53db42619cf3C333b13a18D34fbd9111",
            LoginKeyBodyClientIdIsApiName = true
        };
        Assert.Equal("FinancialAssistant", o.EffectiveLoginKeyBodyClientId);
    }

}

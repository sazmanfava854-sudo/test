using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class ShimasAuthOptionsLoginKeyClientIdTests
{
    [Fact]
    public void EffectiveLoginKeyBodyClientId_uses_apiName_per_sso_doc_when_enabled()
    {
        var o = new ShimasAuthOptions
        {
            ApiName = "FinancialAssistant",
            ClientId = "53db42619cf3C333b13a18D34fbd9111",
            LoginKeyBodyClientIdIsApiName = true
        };
        Assert.Equal("FinancialAssistant", o.EffectiveLoginKeyBodyClientId);
    }

    [Fact]
    public void EffectiveLoginKeyBodyClientId_uses_configured_client_when_legacy_mode()
    {
        var o = new ShimasAuthOptions
        {
            ApiName = "FinancialAssistant",
            ClientId = "53db42619cf3C333b13a18D34fbd9111",
            LoginKeyBodyClientIdIsApiName = false
        };
        Assert.Equal("53db42619cf3C333b13a18D34fbd9111", o.EffectiveLoginKeyBodyClientId);
    }
}

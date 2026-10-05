using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class SsoOutboundFieldMapTests
{
    [Fact]
    public void Describe_maps_apiName_header_and_guid_body()
    {
        var o = new SsoAuthOptions
        {
            ApiName = "FinancialAssistant",
            ClientId = "53db42619cf3C333b13a18D34fbd9111",
            LoginKeyBodyClientIdIsApiName = false
        };
        var map = SsoOutboundFieldMap.Describe(o);
        Assert.Equal("FinancialAssistant", map.HeaderApiName);
        Assert.Equal("53db42619cf3C333b13a18D34fbd9111", map.BodyClientId);
        Assert.False(map.LikelyApiNameClientIdSwappedInConfig);
    }

    [Fact]
    public void LikelySwap_when_guid_in_apiName_and_name_in_clientId()
    {
        var o = new SsoAuthOptions
        {
            ApiName = "53db42619cf3C333b13a18D34fbd9111",
            ClientId = "FinancialAssistant"
        };
        Assert.True(SsoOutboundFieldMap.LikelyApiNameAndClientIdSwapped(o));
    }
}

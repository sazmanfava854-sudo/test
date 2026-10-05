using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class SsoAuthConfigurationTests
{
    [Fact]
    public void ApplyMashhadAliases_reads_Settings_section_like_RuleEngine()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Settings:SSOBaseUrl"] = "https://login.mashhad.ir",
                ["Settings:SSOClientId"] = "4d7475D499c02B3",
                ["Settings:SSOSecret"] = "51377ACFULL",
                ["Settings:SSOUserName"] = "zavabetapp"
            })
            .Build();

        var options = new SsoAuthOptions { ApiName = "", ClientId = "", ClientSecret = "" };
        SsoAuthConfiguration.ApplyMashhadAliases(config, options);

        Assert.Equal("zavabetapp", options.ApiName);
        Assert.Equal("4d7475D499c02B3", options.ClientId);
        Assert.Equal("51377ACFULL", options.ClientSecret);
        Assert.Equal("https://login.mashhad.ir", options.ApiBaseUrl);
    }

    [Fact]
    public void ApplyMashhadAliases_reads_financial_Assist_from_SSOUserName()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Sso:SSOUserName"] = "financial_Assist"
            })
            .Build();

        var options = new SsoAuthOptions { ApiName = "", ClientId = "53db42619cf3C333b13a18D34fbd9111", ClientSecret = "x" };
        SsoAuthConfiguration.ApplyMashhadAliases(config, options);

        Assert.Equal("financial_Assist", options.ApiName);
    }

    [Fact]
    public void BindSsoOptions_reads_legacy_Auth_Shimas_when_Sso_missing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Shimas:ApiName"] = "financial_Assist",
                ["Auth:Shimas:ClientId"] = "legacy-client-id"
            })
            .Build();

        var options = new SsoAuthOptions();
        SsoAuthConfiguration.BindSsoOptions(config, options);
        SsoAuthConfiguration.ApplyMashhadAliases(config, options);

        Assert.Equal("financial_Assist", options.ApiName);
        Assert.Equal("legacy-client-id", options.ClientId);
    }

    [Fact]
    public void BindSsoOptions_Auth_Sso_overrides_legacy_Shimas()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Shimas:ApiName"] = "old",
                ["Auth:Sso:ApiName"] = "financial_Assist"
            })
            .Build();

        var options = new SsoAuthOptions();
        SsoAuthConfiguration.BindSsoOptions(config, options);

        Assert.Equal("financial_Assist", options.ApiName);
    }

    [Fact]
    public void ApplyMashhadAliases_keeps_FinancialAssistant_when_Settings_has_other_app()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Settings:SSOUserName"] = "zavabetapp",
                ["Settings:SSOClientId"] = "other-client-id"
            })
            .Build();

        var options = new SsoAuthOptions
        {
            ApiName = "FinancialAssistant",
            ClientId = "19cf3C33",
            ClientSecret = "D2fbf"
        };
        SsoAuthConfiguration.ApplyMashhadAliases(config, options);
        Assert.Equal("FinancialAssistant", options.ApiName);
        Assert.Equal("19cf3C33", options.ClientId);
        Assert.Equal("D2fbf", options.ClientSecret);
    }
}

using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class ShimasAuthConfigurationTests
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

        var options = new ShimasAuthOptions { ApiName = "", ClientId = "old", ClientSecret = "x" };
        ShimasAuthConfiguration.ApplyMashhadAliases(config, options);

        Assert.Equal("zavabetapp", options.ApiName);
        Assert.Equal("4d7475D499c02B3", options.ClientId);
        Assert.Equal("51377ACFULL", options.ClientSecret);
        Assert.Equal("https://login.mashhad.ir", options.ApiBaseUrl);
    }

    [Fact]
    public void ApplyMashhadAliases_prefers_Auth_Shimas_over_Settings()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Shimas:SSOUserName"] = "rayvarzapp",
                ["Settings:SSOUserName"] = "zavabetapp"
            })
            .Build();

        var options = new ShimasAuthOptions();
        ShimasAuthConfiguration.ApplyMashhadAliases(config, options);
        Assert.Equal("rayvarzapp", options.ApiName);
    }
}

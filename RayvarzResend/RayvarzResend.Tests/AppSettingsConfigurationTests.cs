using Microsoft.Extensions.Configuration;
using RayvarzResend.Web;
using Xunit;

namespace RayvarzResend.Tests;

public class AppSettingsConfigurationTests
{
    [Fact]
    public void UseSingleAppSettingsJsonOnly_removes_environment_specific_json_sources()
    {
        var config = new ConfigurationManager();
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json", optional: true);
        config.AddJsonFile("appsettings.Production.json", optional: true);
        config.AddJsonFile("appsettings.Development.json", optional: true);
        config.AddEnvironmentVariables();

        AppSettingsConfiguration.UseSingleAppSettingsJsonOnly(config);

        var paths = config.Sources
            .OfType<FileConfigurationSource>()
            .Select(s => s.Path)
            .Where(p => p != null)
            .ToList();

        Assert.Contains("appsettings.json", paths);
        Assert.DoesNotContain(paths, p => string.Equals(p, "appsettings.Production.json", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, p => string.Equals(p, "appsettings.Development.json", StringComparison.OrdinalIgnoreCase));
        Assert.True(config.Sources.Count >= 2);
    }
}

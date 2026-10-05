using System.Text.Json;
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

    [Fact]
    public void Appsettings_json_parses()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "appsettings.json"));
        AppSettingsJsonGuard.ValidateOrThrow(Path.GetDirectoryName(path));
    }

    [Fact]
    public void Appsettings_sso_has_client_id_and_secret()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "appsettings.json"));
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var sso = doc.RootElement.GetProperty("Auth").GetProperty("Sso");
        Assert.True(sso.GetProperty("Enabled").GetBoolean());
        Assert.Equal("53db42619cf3C333b13a18D34fbd9111", sso.GetProperty("ClientId").GetString());
        Assert.Equal("D2fbf", sso.GetProperty("ClientSecret").GetString());
        Assert.Equal("financial_Assist", sso.GetProperty("ApiName").GetString());
        Assert.Equal("financial_Assist", sso.GetProperty("SSOUserName").GetString());
        Assert.False(sso.GetProperty("AllowLegacyLoginUrlWithoutLoginKey").GetBoolean());
    }

    [Fact]
    public void Guard_allows_json_comments_like_aspnet_config()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rr-appsettings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "appsettings.json"), """
                {
                  // SSO debug
                  "Auth": { "SessionHours": 8 }
                }
                """);
            AppSettingsJsonGuard.ValidateOrThrow(dir);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Guard_describes_missing_comma_after_connection_strings()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rr-appsettings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "appsettings.json"), """
                {
                  "ConnectionStrings": {
                    "Sara": "Server=.;Database=x;"
                  }
                  "Auth": { "SessionHours": 8 }
                }
                """);
            var ex = Assert.Throws<InvalidDataException>(() => AppSettingsJsonGuard.ValidateOrThrow(dir));
            Assert.Contains("appsettings.json", AppSettingsJsonGuard.Describe(ex, dir));
            Assert.Contains("ویرگول", AppSettingsJsonGuard.Describe(ex, dir));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}

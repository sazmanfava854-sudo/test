using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class SqlConnectionStringResolverTests
{
    [Fact]
    public void Get_builds_named_instance_from_SqlServer_and_SqlInstance()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Security"] = "Database=security10;User Id=hService;Password=secret;TrustServerCertificate=True;",
                ["Shahkar:SqlServer"] = "172.16.8.138",
                ["Shahkar:SqlInstance"] = "sql2012"
            })
            .Build();

        var cs = SqlConnectionStringResolver.Get(config, "Security");
        Assert.NotNull(cs);
        var summary = SqlConnectionStringResolver.Summarize(cs);
        Assert.Equal(@"172.16.8.138\sql2012", summary.DataSource);
        Assert.Equal("security10", summary.Database);
        Assert.Equal("hService", summary.UserId);
        Assert.True(summary.HasPassword);
    }

    [Fact]
    public void Get_fixes_glued_instance_suffix()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Security"] = "Server=172.16.8.138sql2012;Database=security10;User Id=hService;Password=x;",
                ["Shahkar:SqlInstance"] = "sql2012"
            })
            .Build();

        var cs = SqlConnectionStringResolver.Get(config, "Security");
        var summary = SqlConnectionStringResolver.Summarize(cs);
        Assert.Equal(@"172.16.8.138\sql2012", summary.DataSource);
    }

    [Fact]
    public void BuildLoginFailedHint_mentions_json_backslash_when_instance_glued()
    {
        var summary = new ConnectionStringSummary(true, "172.16.8.138sql2012", "security10", false, "hService", true);
        var hint = SqlConnectionStringResolver.BuildLoginFailedHint(
            "Security",
            summary,
            new Exception("Login failed for user 'hService'."));
        Assert.NotNull(hint);
        Assert.Contains("SqlServer", hint);
    }
}

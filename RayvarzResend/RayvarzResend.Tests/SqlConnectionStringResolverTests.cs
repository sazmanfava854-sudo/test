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

    [Fact]
    public void Get_keeps_named_instance_from_ssms_style_string()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Security"] = @"Server=172.16.8.138\sql2012;Database=security10;User Id=hService;Password=secret;TrustServerCertificate=True;",
                ["Shahkar:SqlServer"] = "172.16.8.138",
                ["Shahkar:SqlInstance"] = "sql2012"
            })
            .Build();

        var cs = SqlConnectionStringResolver.Get(config, "Security");
        var summary = SqlConnectionStringResolver.Summarize(cs);
        Assert.Equal(@"172.16.8.138\sql2012", summary.DataSource);
        Assert.Contains("Encrypt=False", cs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Get_password_field_keeps_semicolon()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Security"] = "Database=security10;User Id=hService;Password=CHANGE_ME;",
                ["Shahkar:SqlServer"] = "172.16.8.138",
                ["Shahkar:SqlInstance"] = "sql2012",
                ["Shahkar:Password"] = "a;b;c"
            })
            .Build();

        var cs = SqlConnectionStringResolver.Get(config, "Security");
        Assert.Equal("a;b;c", new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs).Password);
        Assert.Equal(5, SqlConnectionStringResolver.PasswordLength(cs));
    }

    [Fact]
    public void Get_sql_port_bypasses_instance_name()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Security"] = "Database=security10;User Id=hService;Password=x;",
                ["Shahkar:SqlServer"] = "172.16.8.138",
                ["Shahkar:SqlInstance"] = "sql2012",
                ["Shahkar:SqlPort"] = "51433"
            })
            .Build();

        var summary = SqlConnectionStringResolver.Summarize(SqlConnectionStringResolver.Get(config, "Security"));
        Assert.Equal("172.16.8.138,51433", summary.DataSource);
    }

    [Fact]
    public void Refusal_message_includes_grant_when_login_cannot_open_database()
    {
        var summary = new ConnectionStringSummary(true, @"172.16.8.138\sql2012", "security10", false, "hService", true);
        var (message, hint) = ShahkarDatabaseMessages.DescribeRefusal(
            summary,
            8,
            new ShahkarDatabaseProbe(true, "security10", "ONLINE", false, true));
        Assert.Contains("طول رمز: 8", message);
        Assert.Contains(@"172.16.8.138\sql2012", message);
        Assert.Contains("CREATE USER [hService]", hint);
        Assert.Contains("USE [security10]", hint);
    }

    [Fact]
    public void Refusal_message_when_database_missing_on_instance()
    {
        var summary = new ConnectionStringSummary(true, "172.16.8.138", "security10", false, "hService", true);
        var (message, _) = ShahkarDatabaseMessages.DescribeRefusal(
            summary,
            4,
            new ShahkarDatabaseProbe(false, "", "", null, true));
        Assert.Contains("SqlPort", message);
        Assert.Contains("نیست", message);
    }
}

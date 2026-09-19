using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AccountingDocDryRunConfigTests
{
    private static AccountingDocWriter CreateWriter(IConfiguration config) =>
        new(config, new FicheRepository(config), NullLogger<AccountingDocWriter>.Instance);

    [Fact]
    public void AccountingDoc_dry_run_defaults_false_when_rayvarz_key_missing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Sara"] = "Server=.;Database=x;TrustServerCertificate=True;",
                ["ConnectionStrings:Rayvarz"] = "Server=.;Database=y;TrustServerCertificate=True;"
            })
            .Build();
        Assert.False(CreateWriter(config).IsDryRun);
    }

    [Fact]
    public void AccountingDoc_respects_explicit_accounting_dry_run()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Sara"] = "Server=.;Database=x;TrustServerCertificate=True;",
                ["ConnectionStrings:Rayvarz"] = "Server=.;Database=y;TrustServerCertificate=True;",
                ["Rayvarz:DryRun"] = "false",
                ["AccountingDoc:DryRun"] = "true"
            })
            .Build();
        Assert.True(CreateWriter(config).IsDryRun);
    }
}

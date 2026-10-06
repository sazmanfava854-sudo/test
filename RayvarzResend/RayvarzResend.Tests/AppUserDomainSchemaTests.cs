using Xunit;

namespace RayvarzResend.Tests;

public class AppUserDomainSchemaTests
{
    [Fact]
    public void Existing_AppUser_table_gains_domain_and_user_0925569917()
    {
        var sql = File.ReadAllText(RepoFile("database", "07_AppUser.sql"));
        var repo = File.ReadAllText(RepoFile("RayvarzResend.Web", "Services", "AppUserRepository.cs"));

        Assert.Contains("ADD [Domain] NVARCHAR(100)", sql);
        Assert.Contains("SET [Domain] = N'hoseine-sh'", sql);
        Assert.Contains("NationalId = N'0925569917' OR Username = N'0925569917'", sql);

        Assert.Contains("ADD [Domain] NVARCHAR(100)", repo);
        Assert.Contains("hoseine-sh", repo);
        Assert.Contains("0925569917", repo);
        Assert.Contains("AppUser.Domain backfill", repo);

        var alter = sql.IndexOf("ADD [Domain] NVARCHAR(100)", StringComparison.Ordinal);
        var batchBreak = sql.IndexOf("\nGO\n", alter, StringComparison.Ordinal);
        var index = sql.IndexOf("CREATE UNIQUE INDEX UQ_AppUser_Domain", StringComparison.Ordinal);
        Assert.True(alter >= 0 && batchBreak > alter && index > batchBreak);

        var method = repo.Split("public async Task EnsureSchemaAsync")[1].Split("public async Task<int> CountUsersAsync")[0];
        Assert.Contains("TryRunSchemaUpgradeAsync", method);
        Assert.Contains("OBJECT_ID(N'dbo.AppUserGroup', N'U') IS NOT NULL", method);
        Assert.Contains("AppUser.Domain backfill", method);
    }

    private static string RepoFile(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine(
            new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(parts).ToArray()));
        Assert.True(File.Exists(path), path);
        return path;
    }
}

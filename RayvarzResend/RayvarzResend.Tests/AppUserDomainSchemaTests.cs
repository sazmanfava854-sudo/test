using Xunit;

namespace RayvarzResend.Tests;

public class AppUserDomainSchemaTests
{
    [Fact]
    public void Existing_AppUser_table_gains_domain_and_user_0925569917()
    {
        var sql = File.ReadAllText(RepoFile("database", "07_AppUser.sql"));
        var repo = File.ReadAllText(RepoFile("RayvarzResend.Web", "Services", "AppUserRepository.cs"));

        foreach (var src in new[] { sql, repo })
        {
            Assert.Contains("ADD [Domain] NVARCHAR(100)", src);
            Assert.Contains("SET [Domain] = N'0925569917'", src);
            Assert.Contains("NationalId = N'0925569917' OR Username = N'0925569917'", src);
        }

        var alter = sql.IndexOf("ADD [Domain] NVARCHAR(100)", StringComparison.Ordinal);
        var batchBreak = sql.IndexOf("\nGO\n", alter, StringComparison.Ordinal);
        var index = sql.IndexOf("CREATE UNIQUE INDEX UQ_AppUser_Domain", StringComparison.Ordinal);
        Assert.True(alter >= 0 && batchBreak > alter && index > batchBreak);

        var method = repo.Split("public async Task EnsureSchemaAsync")[1].Split("public async Task<int> CountUsersAsync")[0];
        var firstExec = method.IndexOf("ExecuteNonQueryAsync", StringComparison.Ordinal);
        var domainSql = method.IndexOf("const string domainSql", StringComparison.Ordinal);
        var secondExec = method.IndexOf("ExecuteNonQueryAsync", domainSql, StringComparison.Ordinal);
        Assert.True(firstExec >= 0 && domainSql > firstExec && secondExec > domainSql);
    }

    private static string RepoFile(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine(
            new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(parts).ToArray()));
        Assert.True(File.Exists(path), path);
        return path;
    }
}

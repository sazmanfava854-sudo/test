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
    }

    private static string RepoFile(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine(
            new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(parts).ToArray()));
        Assert.True(File.Exists(path), path);
        return path;
    }
}

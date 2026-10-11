using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppAuthServiceTests
{
    [Fact]
    public async Task ToSessionAsync_admin_includes_shahkar()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(
            config,
            memory,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var perms = new AppPermissionService(repo);
        var auth = new AppAuthService(
            repo,
            perms,
            config,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppAuthService>.Instance);

        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "8888888888",
            Password = "Secret@123",
            FirstName = "ادمین",
            LastName = "تست",
            NationalId = "8888888888",
            Domain = "admin-shahkar",
            IsAdmin = true
        });

        var session = await auth.ToSessionAsync(user);
        Assert.True(session.IsAdmin);
        Assert.True(session.CanAccessShahkar);
        Assert.True(session.CanManageUsers);
    }
}

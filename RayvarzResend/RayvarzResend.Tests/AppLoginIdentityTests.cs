using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppLoginIdentityTests
{
    [Fact]
    public async Task Local_login_finds_user_by_domain_column()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(
            config,
            memory,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);

        await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "0012345678",
            Password = "Secret@1234",
            FirstName = "تست",
            LastName = "کاربر",
            NationalId = "0012345678",
            Domain = "alidoost-pa",
            District = "102",
            Position = "",
            IsAdmin = false
        });

        var auth = new AppAuthService(
            repo,
            new AppPermissionService(repo),
            new Microsoft.Extensions.Configuration.ConfigurationManager(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppAuthService>.Instance);

        var byDomain = await auth.ValidateCredentialsAsync("alidoost-pa", "Secret@1234");
        Assert.NotNull(byDomain);
        Assert.Equal("0012345678", byDomain!.NationalId);

        Assert.Null(await auth.ValidateCredentialsAsync("alidoost-pa", "wrong-password"));
    }
}

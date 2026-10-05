using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppLoginIdentityTests
{
    [Fact]
    public async Task Local_login_finds_user_by_domain_account_not_only_national_id()
    {
        var memory = new InMemoryAppUserStore();
        var config = new ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);
        await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "1234567890",
            Password = "Secret@123",
            FirstName = "علی",
            LastName = "دوست",
            NationalId = "1234567890",
            Domain = "alidoost-pa",
            District = "1"
        });

        var auth = new AppAuthService(repo, new AppPermissionService(repo), config,
            NullLogger<AppAuthService>.Instance);

        var byDomain = await auth.ValidateCredentialsAsync("alidoost-pa", "Secret@123");
        Assert.NotNull(byDomain);
        Assert.Equal("1234567890", byDomain!.Username);

        var byNational = await auth.ValidateCredentialsAsync("1234567890", "Secret@123");
        Assert.NotNull(byNational);
    }
}

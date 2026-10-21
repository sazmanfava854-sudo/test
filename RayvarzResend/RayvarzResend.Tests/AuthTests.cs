using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AuthTests
{
    [Fact]
    public async Task Admin_reset_password_updates_login_hash()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(
            config,
            memory,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var auth = new AppAuthService(
            repo,
            new AppPermissionService(repo),
            config,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppAuthService>.Instance);

        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "5566778899",
            Password = "OldPass@12",
            FirstName = "تست",
            LastName = "رمز",
            NationalId = "5566778899",
            Domain = "pwd-test-user",
            District = "1",
            IsAdmin = false
        });

        await repo.ResetPasswordAsync(user.Id, "NewPass@99");
        var okOld = await auth.ValidateCredentialsAsync("pwd-test-user", "OldPass@12");
        var okNew = await auth.ValidateCredentialsAsync("pwd-test-user", "NewPass@99");
        Assert.Null(okOld);
        Assert.NotNull(okNew);
    }

    [Fact]
    public async Task Self_service_change_password_without_current()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(
            config,
            memory,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var auth = new AppAuthService(
            repo,
            new AppPermissionService(repo),
            config,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppAuthService>.Instance);

        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "6677889900",
            Password = "Start@123",
            FirstName = "کاربر",
            LastName = "خود",
            NationalId = "6677889900",
            Domain = "self-pwd-user",
            District = "1",
            IsAdmin = false
        });

        await auth.SetPasswordForUserAsync(user.Id, "Fresh@456");
        var session = await auth.ValidateCredentialsAsync("self-pwd-user", "Fresh@456");
        Assert.NotNull(session);
    }

    [Fact]
    public void PasswordHasher_hash_and_verify_roundtrip()
    {
        var hash = PasswordHasherUtil.Hash("Secret@123");
        Assert.True(PasswordHasherUtil.Verify("Secret@123", hash));
        Assert.False(PasswordHasherUtil.Verify("wrong", hash));
    }

    [Fact]
    public void PasswordHasher_rejects_invalid_stored_format()
    {
        Assert.False(PasswordHasherUtil.Verify("x", ""));
        Assert.False(PasswordHasherUtil.Verify("x", "not-valid"));
        Assert.False(PasswordHasherUtil.IsStoredHashFormat("not-valid"));
    }

    [Fact]
    public void PasswordHasher_recognizes_app_hash_format()
    {
        var hash = PasswordHasherUtil.Hash("x");
        Assert.True(PasswordHasherUtil.IsStoredHashFormat(hash));
    }
}

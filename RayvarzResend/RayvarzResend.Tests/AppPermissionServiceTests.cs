using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppPermissionServiceTests
{
    [Fact]
    public async Task Admin_has_all_permissions()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(
            config,
            memory,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var perms = new AppPermissionService(repo);
        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "9999999999",
            Password = "Secret@123",
            FirstName = "A",
            LastName = "B",
            NationalId = "9999999999",
            IsAdmin = true
        });

        var resolved = await perms.ResolveAsync(user);
        Assert.True(resolved.IsAdmin);
        Assert.True(resolved.CanAccessUnsentFiches);
        Assert.True(resolved.CanAccessInstallment);
        Assert.True(resolved.CanAccessFicheDateChange);
        Assert.True(resolved.CanAccessBankInquiryConfirm);
        Assert.True(resolved.CanManageUsers);
    }

    [Fact]
    public async Task Group_membership_grants_fiche_date_only()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(config, memory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var perms = new AppPermissionService(repo);

        var group = await repo.CreateGroupAsync(new CreateAppUserGroupRequest
        {
            Name = "تاریخ فیش",
            CanAccessFicheDateChange = true
        });
        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "2234567890",
            Password = "Secret@123",
            FirstName = "کاربر",
            LastName = "تاریخ",
            NationalId = "2234567890",
            District = "1",
            IsAdmin = false
        });
        await repo.SetUserGroupsAsync(user.Id, [group.Id]);

        var resolved = await perms.ResolveAsync(user);
        Assert.False(resolved.CanAccessUnsentFiches);
        Assert.False(resolved.CanAccessInstallment);
        Assert.True(resolved.CanAccessFicheDateChange);
        Assert.False(resolved.CanManageUsers);
    }

    [Fact]
    public async Task Group_membership_grants_installment_only()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(config, memory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var perms = new AppPermissionService(repo);

        var group = await repo.CreateGroupAsync(new CreateAppUserGroupRequest
        {
            Name = "خزانه",
            CanAccessInstallment = true
        });
        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "1234567890",
            Password = "Secret@123",
            FirstName = "کاربر",
            LastName = "تست",
            NationalId = "1234567890",
            District = "1",
            IsAdmin = false
        });
        await repo.SetUserGroupsAsync(user.Id, [group.Id]);

        var resolved = await perms.ResolveAsync(user);
        Assert.False(resolved.CanAccessUnsentFiches);
        Assert.True(resolved.CanAccessInstallment);
        Assert.False(resolved.CanAccessFicheDateChange);
        Assert.False(resolved.CanManageUsers);
    }

    [Fact]
    public async Task Group_membership_grants_bank_inquiry_only()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
        config["Auth:UseInMemoryStore"] = "true";
        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(config, memory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AppUserRepository>.Instance);
        var perms = new AppPermissionService(repo);

        var group = await repo.CreateGroupAsync(new CreateAppUserGroupRequest
        {
            Name = "استعلام بانک",
            CanAccessBankInquiryConfirm = true
        });
        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "3234567890",
            Password = "Secret@123",
            FirstName = "کاربر",
            LastName = "بانک",
            NationalId = "3234567890",
            District = "1",
            IsAdmin = false
        });
        await repo.SetUserGroupsAsync(user.Id, [group.Id]);

        var resolved = await perms.ResolveAsync(user);
        Assert.False(resolved.CanAccessUnsentFiches);
        Assert.False(resolved.CanAccessInstallment);
        Assert.False(resolved.CanAccessFicheDateChange);
        Assert.True(resolved.CanAccessBankInquiryConfirm);
        Assert.False(resolved.CanManageUsers);
    }
}

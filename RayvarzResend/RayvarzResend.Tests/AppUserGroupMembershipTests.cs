using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppUserGroupMembershipTests
{
    [Fact]
    public async Task UpdateUserAsync_persists_group_memberships()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:UseInMemoryStore"] = "true"
            })
            .Build();

        var memory = new InMemoryAppUserStore();
        var repo = new AppUserRepository(config, memory, NullLogger<AppUserRepository>.Instance);

        var group = await repo.CreateGroupAsync(new CreateAppUserGroupRequest
        {
            Name = "گروه تست",
            CanAccessInstallment = true
        });

        var user = await repo.CreateUserAsync(new CreateAppUserRequest
        {
            Username = "1234567890",
            Password = "secret1",
            FirstName = "کاربر",
            LastName = "تست",
            NationalId = "1234567890",
            District = "2"
        });

        var updated = await repo.UpdateUserAsync(user.Id, new UpdateAppUserRequest
        {
            IsAdmin = false,
            IsActive = true,
            GroupIds = [group.Id]
        });

        Assert.Equal([group.Id], updated.GroupIds);

        var listed = await repo.ListUsersWithGroupsAsync();
        var row = listed.Single(u => u.Id == user.Id);
        Assert.Equal([group.Id], row.GroupIds);
    }
}

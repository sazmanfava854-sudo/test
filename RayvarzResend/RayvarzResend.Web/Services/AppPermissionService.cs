using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class AppPermissionService
{
    private readonly AppUserRepository _users;

    public AppPermissionService(AppUserRepository users) => _users = users;

    public async Task<UserPermissionsDto> ResolveAsync(AppUserRecord user, CancellationToken ct = default)
    {
        if (user.IsAdmin)
            return UserPermissionsDto.FullAdmin();

        var groupIds = await _users.GetUserGroupIdsAsync(user.Id, ct);
        var groups = await _users.ListGroupsAsync(ct);
        var memberGroups = groups.Where(g => groupIds.Contains(g.Id)).ToList();

        return new UserPermissionsDto
        {
            IsAdmin = false,
            CanAccessUnsentFiches = memberGroups.Any(g => g.CanAccessUnsentFiches),
            CanAccessInstallment = memberGroups.Any(g => g.CanAccessInstallment),
            CanAccessFicheDateChange = memberGroups.Any(g => g.CanAccessFicheDateChange),
            CanManageUsers = memberGroups.Any(g => g.CanManageUsers),
            GroupIds = groupIds
        };
    }

    public async Task<UserPermissionsDto> ResolveForPrincipalAsync(
        System.Security.Claims.ClaimsPrincipal? principal,
        CancellationToken ct = default)
    {
        var id = AppAuthService.GetUserId(principal);
        if (id == null)
            return new UserPermissionsDto();

        var user = await _users.FindByIdAsync(id.Value, ct);
        if (user is not { IsActive: true })
            return new UserPermissionsDto();

        return await ResolveAsync(user, ct);
    }

    public static bool Allows(UserPermissionsDto perms, Func<UserPermissionsDto, bool> check) =>
        perms.IsAdmin || check(perms);
}

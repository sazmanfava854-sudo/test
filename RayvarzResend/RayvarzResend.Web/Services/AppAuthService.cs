using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class AppAuthService
{
    private readonly AppUserRepository _users;
    private readonly AppPermissionService _permissions;
    private readonly IConfiguration _config;
    private readonly ILogger<AppAuthService> _logger;

    public AppAuthService(
        AppUserRepository users,
        AppPermissionService permissions,
        IConfiguration config,
        ILogger<AppAuthService> logger)
    {
        _users = users;
        _permissions = permissions;
        _config = config;
        _logger = logger;
    }

    public async Task EnsureBootstrapAdminAsync(CancellationToken ct = default)
    {
        if (!_users.IsConfigured)
        {
            _logger.LogWarning("App auth DB not configured — login disabled until ConnectionStrings:AppAuth or RayvarzRuleEngine is set");
            return;
        }

        await _users.EnsureSchemaAsync(ct);
        if (await _users.CountUsersAsync(ct) > 0)
            return;

        var username = _config["Auth:BootstrapAdmin:Username"] ?? "admin";
        var password = _config["Auth:BootstrapAdmin:Password"] ?? "Admin@1234";
        var firstName = _config["Auth:BootstrapAdmin:FirstName"] ?? "مدیر";
        var lastName = _config["Auth:BootstrapAdmin:LastName"] ?? "سیستم";

        await _users.CreateUserAsync(new CreateAppUserRequest
        {
            Username = username,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            Position = "مدیر سیستم",
            IsAdmin = true
        }, ct);

        _logger.LogWarning("Bootstrap admin user created: {Username} — change password after first login", username);
    }

    public async Task<AppUserRecord?> ValidateCredentialsAsync(string? username, string? password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _users.FindByUsernameAsync(username, ct);
        if (user == null || !user.IsActive)
            return null;
        return PasswordHasherUtil.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<AuthSessionDto> ToSessionAsync(AppUserRecord user, CancellationToken ct = default)
    {
        var perms = await _permissions.ResolveAsync(user, ct);
        return new AuthSessionDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            NationalId = user.NationalId,
            Position = user.Position,
            District = user.District,
            DisplayName = BuildDisplayName(user),
            IsAdmin = user.IsAdmin,
            CanAccessUnsentFiches = perms.CanAccessUnsentFiches,
            CanAccessInstallment = perms.CanAccessInstallment,
            CanManageUsers = perms.CanManageUsers,
            GroupIds = perms.GroupIds
        };
    }

    public static string BuildDisplayName(AppUserRecord user)
    {
        var full = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(full) ? user.Username : full;
    }

    public static ClaimsPrincipal BuildPrincipal(AppUserRecord user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(AuthClaimTypes.UserId, user.Id.ToString()),
            new(AuthClaimTypes.DisplayName, BuildDisplayName(user)),
            new(AuthClaimTypes.District, user.District ?? ""),
            new(AuthClaimTypes.IsAdmin, user.IsAdmin ? "1" : "0")
        };
        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public static bool IsAdmin(ClaimsPrincipal? user) =>
        user?.IsInRole("Admin") == true
        || user?.FindFirst(AuthClaimTypes.IsAdmin)?.Value == "1";

    public static Guid? GetUserId(ClaimsPrincipal? user)
    {
        var raw = user?.FindFirst(AuthClaimTypes.UserId)?.Value
            ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static string ResolveCommentUserName(ClaimsPrincipal? user)
    {
        var display = user?.FindFirst(AuthClaimTypes.DisplayName)?.Value;
        if (!string.IsNullOrWhiteSpace(display))
            return display.Trim();

        var name = user?.Identity?.Name;
        return string.IsNullOrWhiteSpace(name) ? Environment.UserName : name.Trim();
    }

    public AuthenticationProperties CreateAuthProperties(bool persistent) => new()
    {
        IsPersistent = persistent,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(_config.GetValue("Auth:SessionHours", 8)),
        AllowRefresh = true
    };

    public async Task<AuthSessionDto?> GetSessionAsync(ClaimsPrincipal? principal, CancellationToken ct = default)
    {
        var id = GetUserId(principal);
        if (id == null)
            return null;

        var user = await _users.FindByIdAsync(id.Value, ct);
        return user is { IsActive: true } ? await ToSessionAsync(user, ct) : null;
    }
}

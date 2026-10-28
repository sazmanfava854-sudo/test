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
        var username = _config["Auth:BootstrapAdmin:Username"] ?? "admin";
        var password = _config["Auth:BootstrapAdmin:Password"] ?? "Admin@1234";
        var firstName = _config["Auth:BootstrapAdmin:FirstName"] ?? "مدیر";
        var lastName = _config["Auth:BootstrapAdmin:LastName"] ?? "سیستم";
        var nationalId = _config["Auth:BootstrapAdmin:NationalId"] ?? "1234567890";
        var domain = _config["Auth:BootstrapAdmin:Domain"] ?? "admin";

        if (password == "Admin@1234")
            _logger.LogWarning(
                "Auth:BootstrapAdmin:Password is the shipped default — change the admin password (login is throttled but the default is public)");

        var existing = await _users.FindByUsernameAsync(username, ct);
        if (existing != null && existing.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
        {
            if (await _users.EnsureAdminDomainIfEmptyAsync(username, domain, ct))
                _logger.LogInformation("Bootstrap admin domain set to {Domain} for {Username}", domain, username);

            // بازیابی ورود ادمین بدون SSO — رمز و وضعیت ادمین از appsettings اعمال می‌شود
            if (_config.GetValue("Auth:BootstrapAdmin:ResetPasswordOnStartup", false))
            {
                await _users.ResetPasswordAsync(existing.Id, password, ct);
                await _users.EnsureActiveAdminAsync(existing.Id, ct);
                _logger.LogWarning(
                    "Bootstrap admin {Username}: password reset from appsettings (ResetPasswordOnStartup=true) — set it back to false",
                    username);
            }
            return;
        }

        if (await _users.CountUsersAsync(ct) > 0)
            _logger.LogWarning("Bootstrap admin {Username} not found among existing users — creating it", username);

        await _users.CreateUserAsync(new CreateAppUserRequest
        {
            Username = username,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            NationalId = nationalId,
            Domain = domain,
            Position = "مدیر سیستم",
            IsAdmin = true
        }, ct);

        _logger.LogWarning("Bootstrap admin user created: {Username} — change password after first login", username);
    }

    public async Task<AppUserRecord?> ValidateCredentialsAsync(string? username, string? password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        // ورود با دامین سازمانی (مثلاً alidoost-pa)، کد ملی، یا Username
        var user = await _users.FindBySsoIdentityAsync(username, ct)
            ?? await _users.FindByUsernameAsync(username, ct);
        if (user == null || !user.IsActive)
            return null;
        if (!PasswordHasherUtil.Verify(password, user.PasswordHash))
        {
            if (!PasswordHasherUtil.IsStoredHashFormat(user.PasswordHash))
                _logger.LogWarning(
                    "Login rejected for {Login}: PasswordHash in DB is not app format (expected salt.hash PBKDF2) — use UI reset-password or API",
                    username);
            return null;
        }

        return user;
    }

    public async Task SetPasswordForUserAsync(Guid userId, string? newPassword, CancellationToken ct = default)
    {
        var trimmed = (newPassword ?? "").Trim();
        var policyError = PasswordPolicy.Validate(trimmed);
        if (policyError != null)
            throw new ArgumentException(policyError);

        var user = await _users.FindByIdAsync(userId, ct);
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("کاربر یافت نشد");

        await _users.ResetPasswordAsync(userId, trimmed, ct);
    }

    public Task ChangePasswordAsync(
        Guid userId,
        string? currentPassword,
        string? newPassword,
        CancellationToken ct = default) =>
        SetPasswordForUserAsync(userId, newPassword, ct);

    public async Task<AuthSessionDto> ToSessionAsync(AppUserRecord user, CancellationToken ct = default)
    {
        var perms = user.IsAdmin
            ? UserPermissionsDto.FullAdmin()
            : await _permissions.ResolveAsync(user, ct);
        return new AuthSessionDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            NationalId = user.NationalId,
            Position = user.Position,
            District = user.District,
            Domain = user.Domain,
            DisplayName = BuildDisplayName(user),
            IsAdmin = user.IsAdmin,
            CanAccessUnsentFiches = perms.CanAccessUnsentFiches,
            CanAccessInstallment = perms.CanAccessInstallment,
            CanAccessInstallmentCheckDate = perms.CanAccessInstallmentCheckDate,
            CanAccessFicheDateChange = perms.CanAccessFicheDateChange,
            CanAccessBankInquiryConfirm = perms.CanAccessBankInquiryConfirm,
            CanAccessShahkar = perms.CanAccessShahkar,
            CanAccessCostCenter = perms.CanAccessCostCenter,
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

    private static readonly TimeSpan PrincipalRecheckInterval = TimeSpan.FromSeconds(30);
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, (AppUserRecord? User, DateTime LoadedUtc)> _principalCache = new();

    /// <summary>
    /// کوکی شامل IsAdmin/District است؛ بدون این بررسی غیرفعال‌سازی یا تغییر نقش تا پایان نشست اثر نمی‌کند.
    /// </summary>
    public async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var id = GetUserId(context.Principal);
        if (id == null)
        {
            context.RejectPrincipal();
            return;
        }

        AppUserRecord? user;
        var now = DateTime.UtcNow;
        if (_principalCache.TryGetValue(id.Value, out var cached) && now - cached.LoadedUtc < PrincipalRecheckInterval)
        {
            user = cached.User;
        }
        else
        {
            try
            {
                user = await _users.FindByIdAsync(id.Value, context.HttpContext.RequestAborted);
            }
            catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException or InvalidOperationException)
            {
                // DB hiccup must not sign everyone out; the next request re-checks.
                _logger.LogWarning(ex, "Principal re-check skipped for {UserId} — user DB unavailable", id.Value);
                return;
            }

            _principalCache[id.Value] = (user, now);
        }

        if (user is not { IsActive: true })
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        if (PrincipalMatches(context.Principal, user))
            return;

        context.ReplacePrincipal(BuildPrincipal(user));
        context.ShouldRenew = true;
    }

    public void InvalidatePrincipalCache(Guid userId) => _principalCache.TryRemove(userId, out _);

    internal static bool PrincipalMatches(ClaimsPrincipal? principal, AppUserRecord user) =>
        principal?.FindFirst(AuthClaimTypes.IsAdmin)?.Value == (user.IsAdmin ? "1" : "0")
        && (principal?.FindFirst(AuthClaimTypes.District)?.Value ?? "") == (user.District ?? "")
        && (principal?.FindFirst(AuthClaimTypes.DisplayName)?.Value ?? "") == BuildDisplayName(user)
        && principal?.IsInRole("Admin") == user.IsAdmin;

    public Task SignInAsync(HttpContext http, AppUserRecord user, CancellationToken ct = default)
    {
        return http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            BuildPrincipal(user),
            CreateAuthProperties(persistent: true));
    }
}

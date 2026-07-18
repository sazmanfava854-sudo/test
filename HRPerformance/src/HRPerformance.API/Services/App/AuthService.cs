using HRPerformance.Common;
using HRPerformance.DTOs.Auth;
using HRPerformance.Entities;
using HRPerformance.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HRPerformance.Services.App;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IAuditService auditService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !user.IsActive)
            return ApiResponse<LoginResponse>.Fail("نام کاربری یا رمز عبور اشتباه است");

        try
        {
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return ApiResponse<LoginResponse>.Fail("نام کاربری یا رمز عبور اشتباه است");
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex,
                "Invalid password hash for user {UserName}. Run database/11_RepairAuthentication.sql",
                request.UserName);
            return ApiResponse<LoginResponse>.Fail(
                "رمز ذخیره‌شده نامعتبر است. اسکریپت database/11_RepairAuthentication.sql را اجرا کنید.");
        }

        var (access, refresh, expires) = await _tokenService.GenerateTokensAsync(user, ct);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        await _auditService.LogAsync("Login", "User", user.Id.ToString(), ct: ct);
        var dto = new UserDto(user.Id, user.UserName!, user.Email!, user.FirstName, user.LastName, user.FullName, user.OrganizationId, user.EmployeeId, roles.ToList());
        return ApiResponse<LoginResponse>.Ok(new LoginResponse(access, refresh, expires, dto));
    }

    public async Task<ApiResponse<LoginResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
            return ApiResponse<LoginResponse>.Fail("توکن ارسال نشده است");

        try
        {
            var result = await _tokenService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, ct);
            if (result == null) return ApiResponse<LoginResponse>.Fail("توکن نامعتبر یا منقضی شده است");

            var userId = _tokenService.GetUserIdFromExpiredToken(request.AccessToken);
            if (userId == null) return ApiResponse<LoginResponse>.Fail("توکن نامعتبر است");

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null) return ApiResponse<LoginResponse>.Fail("کاربر یافت نشد");

            var roles = await _userManager.GetRolesAsync(user);
            var dto = new UserDto(user.Id, user.UserName!, user.Email!, user.FirstName, user.LastName, user.FullName, user.OrganizationId, user.EmployeeId, roles.ToList());
            return ApiResponse<LoginResponse>.Ok(new LoginResponse(result.Value.AccessToken, result.Value.RefreshToken, result.Value.ExpiresAt, dto));
        }
        catch (Exception)
        {
            return ApiResponse<LoginResponse>.Fail("خطا در تمدید توکن. دوباره وارد شوید.");
        }
    }
}

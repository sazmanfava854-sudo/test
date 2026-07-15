using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Auth;
using HRPerformance.Application.Interfaces;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HRPerformance.Application.Features.Auth;
public record LoginCommand(LoginRequest Request) : IRequest<ApiResponse<LoginResponse>>;
public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    public LoginCommandHandler(UserManager<ApplicationUser> um, ITokenService ts, IAuditService audit) { _userManager = um; _tokenService = ts; _auditService = audit; }
    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(cmd.Request.UserName);
        if (user == null || !user.IsActive) return ApiResponse<LoginResponse>.Fail("نام کاربری یا رمز عبور اشتباه است");
        if (!await _userManager.CheckPasswordAsync(user, cmd.Request.Password))
            return ApiResponse<LoginResponse>.Fail("نام کاربری یا رمز عبور اشتباه است");
        var (access, refresh, expires) = await _tokenService.GenerateTokensAsync(user, ct);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        await _auditService.LogAsync("Login", "User", user.Id.ToString(), ct: ct);
        var dto = new UserDto(user.Id, user.UserName!, user.Email!, user.FirstName, user.LastName, user.FullName, user.OrganizationId, user.EmployeeId, roles.ToList());
        return ApiResponse<LoginResponse>.Ok(new LoginResponse(access, refresh, expires, dto));
    }
}

using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Auth;
using HRPerformance.Application.Interfaces;
using HRPerformance.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HRPerformance.Application.Features.Auth;
public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<ApiResponse<LoginResponse>>;
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResponse>>
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    public RefreshTokenCommandHandler(ITokenService ts, UserManager<ApplicationUser> um) { _tokenService = ts; _userManager = um; }
    public async Task<ApiResponse<LoginResponse>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var result = await _tokenService.RefreshTokenAsync(cmd.Request.AccessToken, cmd.Request.RefreshToken, ct);
        if (result == null) return ApiResponse<LoginResponse>.Fail("توکن نامعتبر است");
        var userId = _tokenService.GetUserIdFromExpiredToken(cmd.Request.AccessToken);
        if (userId == null) return ApiResponse<LoginResponse>.Fail("توکن نامعتبر است");
        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null) return ApiResponse<LoginResponse>.Fail("کاربر یافت نشد");
        var roles = await _userManager.GetRolesAsync(user);
        var dto = new UserDto(user.Id, user.UserName!, user.Email!, user.FirstName, user.LastName, user.FullName, user.OrganizationId, user.EmployeeId, roles.ToList());
        return ApiResponse<LoginResponse>.Ok(new LoginResponse(result.Value.AccessToken, result.Value.RefreshToken, result.Value.ExpiresAt, dto));
    }
}

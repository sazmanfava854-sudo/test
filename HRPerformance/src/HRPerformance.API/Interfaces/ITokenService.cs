using HRPerformance.Entities;
namespace HRPerformance.Interfaces;
public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokensAsync(ApplicationUser user, CancellationToken ct = default);
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);
    Guid? GetUserIdFromExpiredToken(string token);
}

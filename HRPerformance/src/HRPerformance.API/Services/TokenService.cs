using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRPerformance.Interfaces;
using HRPerformance.Entities;
using HRPerformance.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HRPerformance.Services;
public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    public TokenService(ApplicationDbContext context, IConfiguration config) { _context = context; _config = config; }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokensAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var roles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToListAsync(ct);
        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""), new("firstName", user.FirstName), new("lastName", user.LastName)
        };
        if (user.OrganizationId.HasValue) claims.Add(new Claim("organizationId", user.OrganizationId.Value.ToString()));
        if (user.EmployeeId.HasValue) claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r ?? "")));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60"));
        var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();
        _context.RefreshTokens.Add(new RefreshToken { UserId = user.Id, Token = refreshToken, ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")) });
        await _context.SaveChangesAsync(ct);
        return (accessToken, refreshToken, expires);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        var userId = GetUserIdFromExpiredToken(accessToken);
        if (userId == null) return null;
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId && r.IsActive, ct);
        if (stored == null) return null;
        stored.IsRevoked = true; stored.RevokedAt = DateTime.UtcNow;
        var user = await _context.Users.FindAsync(new object[] { userId.Value }, ct);
        if (user == null) return null;
        return await GenerateTokensAsync(user, ct);
    }

    public Guid? GetUserIdFromExpiredToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try {
            var principal = handler.ValidateToken(token, new TokenValidationParameters {
                ValidateIssuer = true, ValidIssuer = _config["Jwt:Issuer"], ValidateAudience = true, ValidAudience = _config["Jwt:Audience"],
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ValidateLifetime = false
            }, out _);
            var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id != null ? Guid.Parse(id) : null;
        } catch { return null; }
    }

    private static string GenerateRefreshToken() { var bytes = new byte[64]; using var rng = RandomNumberGenerator.Create(); rng.GetBytes(bytes); return Convert.ToBase64String(bytes); }
}

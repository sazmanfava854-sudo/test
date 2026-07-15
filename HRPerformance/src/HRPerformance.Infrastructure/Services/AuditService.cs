using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace HRPerformance.Infrastructure.Services;
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _http;
    public AuditService(ApplicationDbContext ctx, ICurrentUserService cu, IHttpContextAccessor http) { _context = ctx; _currentUser = cu; _http = http; }
    public async Task LogAsync(string action, string? entityType = null, string? entityId = null, string? oldValues = null, string? newValues = null, CancellationToken ct = default)
    {
        _context.AuditLogs.Add(new AuditLog {
            UserId = _currentUser.UserId, UserName = _currentUser.UserName, OrganizationId = _currentUser.OrganizationId,
            Action = action, EntityType = entityType, EntityId = entityId, OldValues = oldValues, NewValues = newValues,
            IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(), UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString()
        });
        await _context.SaveChangesAsync(ct);
    }
}

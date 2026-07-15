using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
namespace HRPerformance.Domain.Interfaces;
public interface IAuditService
{
    Task LogAsync(string action, string? entityType = null, string? entityId = null, string? oldValues = null, string? newValues = null, CancellationToken ct = default);
}

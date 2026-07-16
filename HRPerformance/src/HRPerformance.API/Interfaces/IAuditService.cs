using HRPerformance.Entities;
using HRPerformance.Enums;
namespace HRPerformance.Interfaces;
public interface IAuditService
{
    Task LogAsync(string action, string? entityType = null, string? entityId = null, string? oldValues = null, string? newValues = null, CancellationToken ct = default);
}

using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
namespace HRPerformance.Domain.Interfaces;
public interface IAttendanceSyncService
{
    Task SyncAsync(Guid organizationId, CancellationToken ct = default);
}

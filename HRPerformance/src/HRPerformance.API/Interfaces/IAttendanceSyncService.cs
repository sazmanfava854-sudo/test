using HRPerformance.Entities;
using HRPerformance.Enums;
namespace HRPerformance.Interfaces;
public interface IAttendanceSyncService
{
    Task SyncAsync(Guid organizationId, CancellationToken ct = default);
}

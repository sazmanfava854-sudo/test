using HRPerformance.Domain.Models;

namespace HRPerformance.Domain.Interfaces;

public interface IAttendanceSyncService
{
    Task<AttendanceSyncResult> SyncDateRangeAsync(Guid organizationId, MisSyncDateRangeRequest request, CancellationToken ct = default);
}

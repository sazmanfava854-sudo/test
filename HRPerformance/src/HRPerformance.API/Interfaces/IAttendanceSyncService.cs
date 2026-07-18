using HRPerformance.DTOs.HrIntegration;
using HRPerformance.Services;

namespace HRPerformance.Interfaces;

public interface IAttendanceSyncService
{
    Task<AttendanceSyncResult> SyncDateRangeAsync(Guid organizationId, MisSyncDateRangeRequest request, CancellationToken ct = default);
}

using HRPerformance.Services;
namespace HRPerformance.Interfaces;
public interface IAttendanceSyncService
{
    Task<AttendanceSyncResult> SyncAsync(Guid organizationId, CancellationToken ct = default);
    Task<AttendanceSyncResult> SyncMonthAsync(Guid organizationId, int shamsiYear, int shamsiMonth, CancellationToken ct = default);
}

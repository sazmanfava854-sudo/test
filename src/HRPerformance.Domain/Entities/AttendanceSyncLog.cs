using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Entities;

public class AttendanceSyncLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public DateTime SyncStartedAt { get; set; }
    public DateTime? SyncCompletedAt { get; set; }
    public AttendanceSyncStatus Status { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public int EmployeesInserted { get; set; }
    public int EmployeesUpdated { get; set; }
    public string? SyncType { get; set; }
    public string? RequestedByUserName { get; set; }
    public string? ErrorMessage { get; set; }
    public string SourceType { get; set; } = "REST";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

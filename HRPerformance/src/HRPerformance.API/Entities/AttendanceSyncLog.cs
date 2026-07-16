using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class AttendanceSyncLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public DateTime SyncStartedAt { get; set; }
    public DateTime? SyncCompletedAt { get; set; }
    public AttendanceSyncStatus Status { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string SourceType { get; set; } = "REST";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

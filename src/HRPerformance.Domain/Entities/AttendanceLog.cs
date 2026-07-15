using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Entities;

public class AttendanceLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public TimeSpan? EntryTime { get; set; }
    public TimeSpan? ExitTime { get; set; }
    public decimal? WorkingHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public int DelayMinutes { get; set; }
    public bool IsAbsent { get; set; }
    public bool IsOnMission { get; set; }
    public bool IsOnLeave { get; set; }
    public string? LeaveType { get; set; }
    public string Source { get; set; } = "Sync";
    public string? ExternalId { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Employee? Employee { get; set; }
}

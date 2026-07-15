using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Entities;

public class EmployeeTimeline
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Guid OrganizationId { get; set; }
    public TimelineEventType EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTime EventDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Employee? Employee { get; set; }
}

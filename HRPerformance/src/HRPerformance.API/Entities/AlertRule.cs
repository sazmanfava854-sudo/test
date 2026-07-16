using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class AlertRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AlertType AlertType { get; set; }
    public decimal? Threshold { get; set; }
    public string? Condition { get; set; }
    public bool NotifyEmployee { get; set; } = true;
    public bool NotifyManager { get; set; } = true;
    public bool NotifyAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
}

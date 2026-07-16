using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class Appeal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Guid? ScoreId { get; set; }
    public Guid? EvaluationId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppealStatus Status { get; set; } = AppealStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Employee? Employee { get; set; }
}

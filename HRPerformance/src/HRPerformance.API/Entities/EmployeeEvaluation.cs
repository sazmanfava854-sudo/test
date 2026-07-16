using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class EmployeeEvaluation : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid EvaluatorId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Score { get; set; }
    public ScoreType ScoreType { get; set; }
    public string? Notes { get; set; }
    public DateTime EvaluationDate { get; set; }
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.Pending;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalComments { get; set; }
    public Employee? Employee { get; set; }
    public Employee? Evaluator { get; set; }
}

using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class EvaluationRule : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RuleConditionType ConditionType { get; set; }
    public RuleOperator Operator { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? StringValue { get; set; }
    public decimal ScoreImpact { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public Organization? Organization { get; set; }
    public EvaluationCategory? Category { get; set; }
    public EvaluationItem? Item { get; set; }
}

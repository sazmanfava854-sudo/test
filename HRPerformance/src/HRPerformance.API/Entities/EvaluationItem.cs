using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class EvaluationItem : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ScoreType ScoreType { get; set; }
    public decimal DefaultScore { get; set; }
    public decimal MaxScore { get; set; } = 100;
    public decimal MinScore { get; set; } = -100;
    public decimal Weight { get; set; } = 1;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int Priority { get; set; }
    public bool IsAutoCalculated { get; set; }
    public bool IsActive { get; set; } = true;
    public EvaluationCategory? Category { get; set; }
}

using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class EvaluationCategory : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public decimal Weight { get; set; } = 1;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Organization? Organization { get; set; }
    public ICollection<EvaluationItem> Items { get; set; } = new List<EvaluationItem>();
}

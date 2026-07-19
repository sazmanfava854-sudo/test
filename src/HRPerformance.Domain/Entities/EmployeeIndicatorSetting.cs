using HRPerformance.Domain.Common;

namespace HRPerformance.Domain.Entities;

public class EmployeeIndicatorSetting : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Weight { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public Employee? Employee { get; set; }
    public EvaluationCategory? Category { get; set; }
}

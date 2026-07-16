using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class Ranking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public int Rank { get; set; }
    public decimal Score { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public PeriodType PeriodType { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

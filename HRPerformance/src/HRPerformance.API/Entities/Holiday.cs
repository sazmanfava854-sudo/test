using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class Holiday
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public bool IsRecurring { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
}

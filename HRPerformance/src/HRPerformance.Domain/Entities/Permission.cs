using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

using HRPerformance.Common;
using HRPerformance.Enums;
using Microsoft.AspNetCore.Identity;

namespace HRPerformance.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

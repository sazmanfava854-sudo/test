using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace HRPerformance.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

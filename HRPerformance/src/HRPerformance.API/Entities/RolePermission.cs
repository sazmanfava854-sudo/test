using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public ApplicationRole? Role { get; set; }
    public Permission? Permission { get; set; }
}

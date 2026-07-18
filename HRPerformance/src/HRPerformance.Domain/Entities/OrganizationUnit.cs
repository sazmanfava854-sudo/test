using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Entities;

public class OrganizationUnit : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public OrganizationUnitType UnitType { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public Guid? ManagerId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Organization? Organization { get; set; }
    public OrganizationUnit? Parent { get; set; }
    public ICollection<OrganizationUnit> Children { get; set; } = new List<OrganizationUnit>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

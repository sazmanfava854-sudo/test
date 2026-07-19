using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Entities;

public class Employee : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? UserId { get; set; }
    public string PersonnelCode { get; set; } = string.Empty;
    public string NationalCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime EmploymentDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public string? Position { get; set; }
    public string? PhotoPath { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? Description { get; set; }
    public decimal CurrentScore { get; set; }
    public decimal MonthlyScore { get; set; }
    public decimal YearlyScore { get; set; }
    public int? Ranking { get; set; }
    public Organization? Organization { get; set; }
    public OrganizationUnit? OrganizationUnit { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public string FullName => $"{FirstName} {LastName}";
}

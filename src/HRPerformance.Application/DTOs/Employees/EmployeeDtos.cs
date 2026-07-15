using HRPerformance.Domain.Enums;
namespace HRPerformance.Application.DTOs.Employees;
public record EmployeeDto(Guid Id, string PersonnelCode, string NationalCode, string FirstName, string LastName, string FullName,
    string? Position, EmployeeStatus Status, decimal CurrentScore, decimal MonthlyScore, decimal YearlyScore, int? Ranking,
    string? DepartmentName, string? ManagerName, DateTime EmploymentDate, string? PhotoPath);
public record CreateEmployeeRequest(string PersonnelCode, string NationalCode, string FirstName, string LastName, string? FatherName,
    DateTime? BirthDate, string? Phone, string? Email, string? Address, DateTime EmploymentDate, EmploymentType EmploymentType,
    Guid? OrganizationUnitId, Guid? ManagerId, string? Position, string? Description);
public record UpdateEmployeeRequest(Guid Id, string FirstName, string LastName, string? FatherName, DateTime? BirthDate,
    string? Phone, string? Email, string? Address, Guid? OrganizationUnitId, Guid? ManagerId, string? Position, EmployeeStatus Status, string? Description);
public record EmployeeSearchRequest(string? SearchTerm, Guid? DepartmentId, EmployeeStatus? Status, int PageNumber = 1, int PageSize = 20);

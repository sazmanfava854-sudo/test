using HRPerformance.DTOs.Employees;
using HRPerformance.Entities;

namespace HRPerformance.Services.App;

public static class EmployeeMapper
{
    public static EmployeeDto ToDto(Employee e) => new(
        e.Id, e.PersonnelCode, e.NationalCode, e.FirstName, e.LastName, e.FullName,
        e.Position, e.Status, e.CurrentScore, e.MonthlyScore, e.YearlyScore, e.Ranking,
        e.OrganizationUnit?.Name, e.Manager?.FullName, e.EmploymentDate, e.PhotoPath);

    public static Employee FromCreate(CreateEmployeeRequest r) => new()
    {
        PersonnelCode = r.PersonnelCode,
        NationalCode = r.NationalCode,
        FirstName = r.FirstName,
        LastName = r.LastName,
        FatherName = r.FatherName,
        BirthDate = r.BirthDate,
        Phone = r.Phone,
        Email = r.Email,
        Address = r.Address,
        EmploymentDate = r.EmploymentDate,
        EmploymentType = r.EmploymentType,
        OrganizationUnitId = r.OrganizationUnitId,
        ManagerId = r.ManagerId,
        Position = r.Position,
        Description = r.Description
    };

    public static void ApplyUpdate(Employee e, UpdateEmployeeRequest r)
    {
        e.FirstName = r.FirstName;
        e.LastName = r.LastName;
        e.FatherName = r.FatherName;
        e.BirthDate = r.BirthDate;
        e.Phone = r.Phone;
        e.Email = r.Email;
        e.Address = r.Address;
        e.OrganizationUnitId = r.OrganizationUnitId;
        e.ManagerId = r.ManagerId;
        e.Position = r.Position;
        e.Status = r.Status;
        e.Description = r.Description;
    }
}

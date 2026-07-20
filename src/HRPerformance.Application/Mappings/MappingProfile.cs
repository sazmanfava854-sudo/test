using AutoMapper;
using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Domain.Entities;

namespace HRPerformance.Application.Mappings;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ConstructUsing(s => new EmployeeDto(
                s.Id,
                s.PersonnelCode,
                s.NationalCode,
                s.FirstName,
                s.LastName,
                s.FullName,
                s.Position,
                s.Status,
                s.CurrentScore,
                s.MonthlyScore,
                s.YearlyScore,
                s.Ranking,
                s.OrganizationUnit != null ? s.OrganizationUnit.Name : null,
                s.Manager != null ? s.Manager.FullName : null,
                s.EmploymentDate,
                s.PhotoPath));
        CreateMap<CreateEmployeeRequest, Employee>();
        CreateMap<UpdateEmployeeRequest, Employee>();
    }
}

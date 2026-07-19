using AutoMapper;
using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Domain.Entities;

namespace HRPerformance.Application.Mappings;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.OrganizationUnit != null ? s.OrganizationUnit.Name : null))
            .ForMember(d => d.ManagerName, o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : null));
        CreateMap<CreateEmployeeRequest, Employee>();
        CreateMap<UpdateEmployeeRequest, Employee>();
    }
}

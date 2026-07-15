using AutoMapper;
using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Employees;
public record GetEmployeesQuery(EmployeeSearchRequest Request, Guid OrganizationId) : IRequest<ApiResponse<PagedResult<EmployeeDto>>>;
public record GetEmployeeByIdQuery(Guid Id) : IRequest<ApiResponse<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, ApiResponse<PagedResult<EmployeeDto>>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public GetEmployeesQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }
    public async Task<ApiResponse<PagedResult<EmployeeDto>>> Handle(GetEmployeesQuery q, CancellationToken ct)
    {
        var query = _uow.Repository<Employee>().Query()
            .Include(e => e.OrganizationUnit).Include(e => e.Manager)
            .Where(e => e.OrganizationId == q.OrganizationId && !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q.Request.SearchTerm))
        { var t = q.Request.SearchTerm; query = query.Where(e => e.FirstName.Contains(t) || e.LastName.Contains(t) || e.PersonnelCode.Contains(t) || e.NationalCode.Contains(t)); }
        if (q.Request.DepartmentId.HasValue) query = query.Where(e => e.OrganizationUnitId == q.Request.DepartmentId);
        if (q.Request.Status.HasValue) query = query.Where(e => e.Status == q.Request.Status);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(e => e.LastName).Skip((q.Request.PageNumber - 1) * q.Request.PageSize).Take(q.Request.PageSize).ToListAsync(ct);
        return ApiResponse<PagedResult<EmployeeDto>>.Ok(new PagedResult<EmployeeDto> { Items = _mapper.Map<List<EmployeeDto>>(items), TotalCount = total, PageNumber = q.Request.PageNumber, PageSize = q.Request.PageSize });
    }
}

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, ApiResponse<EmployeeDto>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public GetEmployeeByIdQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }
    public async Task<ApiResponse<EmployeeDto>> Handle(GetEmployeeByIdQuery q, CancellationToken ct)
    {
        var emp = await _uow.Repository<Employee>().Query().Include(e => e.OrganizationUnit).Include(e => e.Manager).FirstOrDefaultAsync(e => e.Id == q.Id && !e.IsDeleted, ct);
        return emp == null ? ApiResponse<EmployeeDto>.Fail("کارمند یافت نشد") : ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(emp));
    }
}

using AutoMapper;
using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Employees;
public record GetEmployeesQuery(EmployeeSearchRequest Request, Guid OrganizationId) : IRequest<ApiResponse<PagedResult<EmployeeDto>>>;
public record GetEmployeeLookupQuery(EmployeeLookupRequest Request, Guid OrganizationId) : IRequest<ApiResponse<PagedResult<EmployeeLookupDto>>>;
public record GetEmployeeByIdQuery(Guid Id) : IRequest<ApiResponse<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, ApiResponse<PagedResult<EmployeeDto>>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public GetEmployeesQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }
    public async Task<ApiResponse<PagedResult<EmployeeDto>>> Handle(GetEmployeesQuery q, CancellationToken ct)
    {
        if (q.OrganizationId == Guid.Empty)
            return ApiResponse<PagedResult<EmployeeDto>>.Fail("شناسه سازمان یافت نشد — دوباره وارد شوید");

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

public class GetEmployeeLookupQueryHandler : IRequestHandler<GetEmployeeLookupQuery, ApiResponse<PagedResult<EmployeeLookupDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetEmployeeLookupQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<PagedResult<EmployeeLookupDto>>> Handle(GetEmployeeLookupQuery q, CancellationToken ct)
    {
        if (q.OrganizationId == Guid.Empty)
            return ApiResponse<PagedResult<EmployeeLookupDto>>.Fail("شناسه سازمان یافت نشد — دوباره وارد شوید");

        var pageSize = Math.Clamp(q.Request.PageSize, 1, 50);
        var pageNumber = Math.Max(1, q.Request.PageNumber);

        var query = _uow.Repository<Employee>().Query()
            .Where(e => e.OrganizationId == q.OrganizationId && !e.IsDeleted && e.Status == EmployeeStatus.Active);

        if (!string.IsNullOrWhiteSpace(q.Request.Query))
        {
            var term = q.Request.Query.Trim();
            query = query.Where(e =>
                e.FirstName.Contains(term) ||
                e.LastName.Contains(term) ||
                e.PersonnelCode.Contains(term) ||
                (e.FirstName + " " + e.LastName).Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeLookupDto(e.Id, e.PersonnelCode, e.FirstName + " " + e.LastName))
            .ToListAsync(ct);

        return ApiResponse<PagedResult<EmployeeLookupDto>>.Ok(new PagedResult<EmployeeLookupDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
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

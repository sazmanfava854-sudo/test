using HRPerformance.Common;
using HRPerformance.DTOs.Employees;
using HRPerformance.Entities;
using HRPerformance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.App;

public class EmployeeService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public EmployeeService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<ApiResponse<PagedResult<EmployeeDto>>> GetAllAsync(EmployeeSearchRequest request, Guid organizationId, CancellationToken ct = default)
    {
        if (organizationId == Guid.Empty)
            return ApiResponse<PagedResult<EmployeeDto>>.Fail("شناسه سازمان در توکن یافت نشد. یک بار خارج و دوباره وارد شوید.");
        var query = _uow.Repository<Employee>().Query()
            .Include(e => e.OrganizationUnit).Include(e => e.Manager)
            .Where(e => e.OrganizationId == organizationId && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var t = request.SearchTerm;
            query = query.Where(e => e.FirstName.Contains(t) || e.LastName.Contains(t) || e.PersonnelCode.Contains(t) || e.NationalCode.Contains(t));
        }
        if (request.DepartmentId.HasValue) query = query.Where(e => e.OrganizationUnitId == request.DepartmentId);
        if (request.Status.HasValue) query = query.Where(e => e.Status == request.Status);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(e => e.LastName)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);

        return ApiResponse<PagedResult<EmployeeDto>>.Ok(new PagedResult<EmployeeDto>
        {
            Items = items.Select(EmployeeMapper.ToDto).ToList(),
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }

    public async Task<ApiResponse<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Repository<Employee>().Query()
            .Include(e => e.OrganizationUnit).Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        return emp == null
            ? ApiResponse<EmployeeDto>.Fail("کارمند یافت نشد")
            : ApiResponse<EmployeeDto>.Ok(EmployeeMapper.ToDto(emp));
    }

    public async Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, Guid organizationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PersonnelCode) || string.IsNullOrWhiteSpace(request.NationalCode))
            return ApiResponse<EmployeeDto>.Fail("کد پرسنلی و کد ملی الزامی است");

        var emp = EmployeeMapper.FromCreate(request);
        emp.OrganizationId = organizationId;
        await _uow.Repository<Employee>().AddAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", "Employee", emp.Id.ToString(), ct: ct);
        return ApiResponse<EmployeeDto>.Ok(EmployeeMapper.ToDto(emp));
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateAsync(UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var emp = await _uow.Repository<Employee>().GetByIdAsync(request.Id, ct);
        if (emp == null || emp.IsDeleted) return ApiResponse<EmployeeDto>.Fail("کارمند یافت نشد");

        EmployeeMapper.ApplyUpdate(emp, request);
        emp.UpdatedAt = DateTime.UtcNow;
        await _uow.Repository<Employee>().UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", "Employee", emp.Id.ToString(), ct: ct);
        return ApiResponse<EmployeeDto>.Ok(EmployeeMapper.ToDto(emp));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Repository<Employee>().GetByIdAsync(id, ct);
        if (emp == null) return ApiResponse<bool>.Fail("کارمند یافت نشد");

        emp.IsDeleted = true;
        emp.UpdatedAt = DateTime.UtcNow;
        await _uow.Repository<Employee>().UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", "Employee", id.ToString(), ct: ct);
        return ApiResponse<bool>.Ok(true);
    }
}

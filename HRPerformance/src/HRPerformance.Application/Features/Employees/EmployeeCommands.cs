using AutoMapper;
using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using MediatR;

namespace HRPerformance.Application.Features.Employees;
public record CreateEmployeeCommand(CreateEmployeeRequest Request, Guid OrganizationId) : IRequest<ApiResponse<EmployeeDto>>;
public record UpdateEmployeeCommand(UpdateEmployeeRequest Request) : IRequest<ApiResponse<EmployeeDto>>;
public record DeleteEmployeeCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ApiResponse<EmployeeDto>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper; private readonly IAuditService _audit;
    public CreateEmployeeCommandHandler(IUnitOfWork uow, IMapper mapper, IAuditService audit) { _uow = uow; _mapper = mapper; _audit = audit; }
    public async Task<ApiResponse<EmployeeDto>> Handle(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        var emp = _mapper.Map<Employee>(cmd.Request);
        emp.OrganizationId = cmd.OrganizationId;
        await _uow.Repository<Employee>().AddAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", "Employee", emp.Id.ToString(), ct: ct);
        return ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(emp));
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ApiResponse<EmployeeDto>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper; private readonly IAuditService _audit;
    public UpdateEmployeeCommandHandler(IUnitOfWork uow, IMapper mapper, IAuditService audit) { _uow = uow; _mapper = mapper; _audit = audit; }
    public async Task<ApiResponse<EmployeeDto>> Handle(UpdateEmployeeCommand cmd, CancellationToken ct)
    {
        var emp = await _uow.Repository<Employee>().GetByIdAsync(cmd.Request.Id, ct);
        if (emp == null || emp.IsDeleted) return ApiResponse<EmployeeDto>.Fail("کارمند یافت نشد");
        _mapper.Map(cmd.Request, emp);
        emp.UpdatedAt = DateTime.UtcNow;
        await _uow.Repository<Employee>().UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", "Employee", emp.Id.ToString(), ct: ct);
        return ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(emp));
    }
}

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _uow; private readonly IAuditService _audit;
    public DeleteEmployeeCommandHandler(IUnitOfWork uow, IAuditService audit) { _uow = uow; _audit = audit; }
    public async Task<ApiResponse<bool>> Handle(DeleteEmployeeCommand cmd, CancellationToken ct)
    {
        var emp = await _uow.Repository<Employee>().GetByIdAsync(cmd.Id, ct);
        if (emp == null) return ApiResponse<bool>.Fail("کارمند یافت نشد");
        emp.IsDeleted = true; emp.UpdatedAt = DateTime.UtcNow;
        await _uow.Repository<Employee>().UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", "Employee", cmd.Id.ToString(), ct: ct);
        return ApiResponse<bool>.Ok(true);
    }
}

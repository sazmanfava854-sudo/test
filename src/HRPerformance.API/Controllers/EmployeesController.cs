using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Application.Features.Employees;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly MisEmployeeRosterSyncService _rosterSync;
    private readonly ApplicationDbContext _context;

    public EmployeesController(
        IMediator mediator,
        ICurrentUserService currentUser,
        MisEmployeeRosterSyncService rosterSync,
        ApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _rosterSync = rosterSync;
        _context = context;
    }
    [HttpGet]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeSearchRequest request) =>
        Ok(await _mediator.Send(new GetEmployeesQuery(request, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpGet("lookup")]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> Lookup([FromQuery] EmployeeLookupRequest request) =>
        Ok(await _mediator.Send(new GetEmployeeLookupQuery(request, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpGet("summary")]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد — logout/login کنید" });

        var settings = await _context.AttendanceIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId, ct);

        var count = await _context.Employees.CountAsync(e => e.OrganizationId == orgId && !e.IsDeleted, ct);
        return Ok(new
        {
            success = true,
            organizationId = orgId,
            employeesInDatabase = count,
            lastEmployeeRosterSyncAt = settings?.LastEmployeeRosterSyncAt,
            isRosterSyncRunning = settings?.IsRosterSyncRunning ?? false,
            provinceCode = settings?.ProvinceCode ?? "147"
        });
    }
    [HttpPost("sync-from-mis")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> SyncFromMis(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد — دوباره وارد شوید" });

        var result = await _rosterSync.SyncFromMisAsync(orgId, _currentUser.UserId, _currentUser.UserName, ct);
        if (!result.Success && result.Inserted == 0 && result.Updated == 0)
        {
            if (result.ErrorMessage?.Contains("در حال اجرا") == true)
                return Conflict(new { success = false, message = result.ErrorMessage });
            return BadRequest(new { success = false, message = result.ErrorMessage ?? "خطا در دریافت فهرست پرسنل" });
        }

        return Ok(new
        {
            success = result.Success,
            message = $"فهرست پرسنل: {result.Inserted} جدید، {result.Updated} به‌روز، {result.Total} از MIS",
            inserted = result.Inserted,
            updated = result.Updated,
            total = result.Total,
            durationMs = result.DurationMs,
            provinceCode = result.ProvinceCode,
            lastSyncAt = result.LastSyncAt
        });
    }
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _mediator.Send(new GetEmployeeByIdQuery(id)));
    [HttpPost] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator,Manager")] public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request) =>
        Ok(await _mediator.Send(new CreateEmployeeCommand(request, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpPut] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator,Manager")] public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest request) =>
        Ok(await _mediator.Send(new UpdateEmployeeCommand(request)));
    [HttpDelete("{id:guid}")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> Delete(Guid id) =>
        Ok(await _mediator.Send(new DeleteEmployeeCommand(id)));
}

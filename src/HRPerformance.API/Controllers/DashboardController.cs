using HRPerformance.Application.Features.Dashboard;
using HRPerformance.Application.Interfaces;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmployeeAccountLinkService _employeeLink;

    public DashboardController(
        IMediator mediator,
        ICurrentUserService currentUser,
        IEmployeeAccountLinkService employeeLink)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _employeeLink = employeeLink;
    }

    [HttpGet("employee")]
    public async Task<IActionResult> EmployeeDashboard(CancellationToken ct)
    {
        var employeeId = await _employeeLink.ResolveEmployeeIdAsync(
            _currentUser.UserId ?? Guid.Empty,
            _currentUser.UserName,
            _currentUser.OrganizationId,
            ct) ?? Guid.Empty;

        return Ok(await _mediator.Send(new GetEmployeeDashboardQuery(employeeId), ct));
    }
    [HttpGet("manager")] [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> ManagerDashboard() =>
        Ok(await _mediator.Send(new GetManagerDashboardQuery(_currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpGet("admin")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> AdminDashboard() =>
        Ok(await _mediator.Send(new GetAdminDashboardQuery(_currentUser.OrganizationId ?? Guid.Empty)));
}

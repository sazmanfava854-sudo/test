using HRPerformance.Application.Features.Dashboard;
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
    public DashboardController(IMediator mediator, ICurrentUserService currentUser) { _mediator = mediator; _currentUser = currentUser; }
    [HttpGet("employee")] public async Task<IActionResult> EmployeeDashboard() =>
        Ok(await _mediator.Send(new GetEmployeeDashboardQuery(_currentUser.EmployeeId ?? Guid.Empty)));
    [HttpGet("manager")] [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> ManagerDashboard() =>
        Ok(await _mediator.Send(new GetManagerDashboardQuery(_currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpGet("admin")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> AdminDashboard() =>
        Ok(await _mediator.Send(new GetAdminDashboardQuery(_currentUser.OrganizationId ?? Guid.Empty)));
}

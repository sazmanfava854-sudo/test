using HRPerformance.Interfaces;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(DashboardService dashboardService, ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }

    [HttpGet("employee")]
    public async Task<IActionResult> EmployeeDashboard() =>
        Ok(await _dashboardService.GetEmployeeDashboardAsync(_currentUser.EmployeeId ?? Guid.Empty));

    [HttpGet("manager")]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> ManagerDashboard() =>
        Ok(await _dashboardService.GetManagerDashboardAsync(_currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty));

    [HttpGet("admin")]
    [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> AdminDashboard() =>
        Ok(await _dashboardService.GetAdminDashboardAsync(_currentUser.OrganizationId ?? Guid.Empty));
}

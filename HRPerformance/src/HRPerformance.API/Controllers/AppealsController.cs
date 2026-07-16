using HRPerformance.DTOs.Appeals;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppealsController : ControllerBase
{
    private readonly AppealService _appealService;
    private readonly ICurrentUserService _currentUser;

    public AppealsController(AppealService appealService, ICurrentUserService currentUser)
    {
        _appealService = appealService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AppealStatus? status) =>
        Ok(await _appealService.GetAllAsync(_currentUser.OrganizationId ?? Guid.Empty, status));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppealRequest request) =>
        Ok(await _appealService.CreateAsync(request, _currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty));

    [HttpPut("review")]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> Review([FromBody] ReviewAppealRequest request) =>
        Ok(await _appealService.ReviewAsync(request, _currentUser.UserId ?? Guid.Empty));
}

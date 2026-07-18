using HRPerformance.DTOs.HrIntegration;
using HRPerformance.Interfaces;
using HRPerformance.Services.ExternalHr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
public class HrIntegrationController : ControllerBase
{
    private readonly HrIntegrationSettingsService _settingsService;
    private readonly ICurrentUserService _currentUser;

    public HrIntegrationController(HrIntegrationSettingsService settingsService, ICurrentUserService currentUser)
    {
        _settingsService = settingsService;
        _currentUser = currentUser;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var settings = await _settingsService.GetDtoAsync(orgId, ct);
        return Ok(new { success = true, data = settings });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateHrIntegrationSettingsRequest request, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var settings = await _settingsService.UpdateAsync(orgId, request, ct);
        return Ok(new { success = true, message = "تنظیمات سینک MIS ذخیره شد", data = settings });
    }
}

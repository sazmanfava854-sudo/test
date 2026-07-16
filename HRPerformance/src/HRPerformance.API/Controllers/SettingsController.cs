using HRPerformance.DTOs.Settings;
using HRPerformance.Interfaces;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
public class SettingsController : ControllerBase
{
    private readonly SettingService _settingService;
    private readonly ICurrentUserService _currentUser;

    public SettingsController(SettingService settingService, ICurrentUserService currentUser)
    {
        _settingService = settingService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category) =>
        Ok(await _settingService.GetAllAsync(_currentUser.OrganizationId, category));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingRequest request) =>
        Ok(await _settingService.UpdateAsync(_currentUser.OrganizationId, request));

    [HttpGet("holidays")]
    public async Task<IActionResult> GetHolidays() =>
        Ok(await _settingService.GetHolidaysAsync(_currentUser.OrganizationId ?? Guid.Empty));

    [HttpPost("holidays")]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest req) =>
        Ok(await _settingService.CreateHolidayAsync(_currentUser.OrganizationId ?? Guid.Empty, req.Title, req.HolidayDate, req.IsRecurring));
}

public record CreateHolidayRequest(string Title, DateTime HolidayDate, bool IsRecurring);

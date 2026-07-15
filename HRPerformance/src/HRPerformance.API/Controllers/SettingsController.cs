using HRPerformance.Application.DTOs.Settings;
using HRPerformance.Application.Features.Settings;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    public SettingsController(IMediator mediator, ICurrentUserService currentUser) { _mediator = mediator; _currentUser = currentUser; }
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? category) =>
        Ok(await _mediator.Send(new GetSettingsQuery(_currentUser.OrganizationId, category)));
    [HttpPut] public async Task<IActionResult> Update([FromBody] UpdateSettingRequest request) =>
        Ok(await _mediator.Send(new UpdateSettingCommand(_currentUser.OrganizationId, request)));
    [HttpGet("holidays")] public async Task<IActionResult> GetHolidays() =>
        Ok(await _mediator.Send(new GetHolidaysQuery(_currentUser.OrganizationId ?? Guid.Empty)));
    [HttpPost("holidays")] public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest req) =>
        Ok(await _mediator.Send(new CreateHolidayCommand(_currentUser.OrganizationId ?? Guid.Empty, req.Title, req.HolidayDate, req.IsRecurring)));
}
public record CreateHolidayRequest(string Title, DateTime HolidayDate, bool IsRecurring);

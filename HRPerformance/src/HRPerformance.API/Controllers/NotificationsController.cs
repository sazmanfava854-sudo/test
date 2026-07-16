using HRPerformance.Interfaces;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationAppService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(NotificationAppService notificationService, ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? unreadOnly) =>
        Ok(await _notificationService.GetAllAsync(_currentUser.UserId ?? Guid.Empty, unreadOnly));

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id) =>
        Ok(await _notificationService.MarkReadAsync(id));
}

using HRPerformance.Application.Features.Notifications;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    public NotificationsController(IMediator mediator, ICurrentUserService currentUser) { _mediator = mediator; _currentUser = currentUser; }
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool? unreadOnly) =>
        Ok(await _mediator.Send(new GetNotificationsQuery(_currentUser.UserId ?? Guid.Empty, unreadOnly)));
    [HttpPut("{id:guid}/read")] public async Task<IActionResult> MarkRead(Guid id) =>
        Ok(await _mediator.Send(new MarkNotificationReadCommand(id)));
}

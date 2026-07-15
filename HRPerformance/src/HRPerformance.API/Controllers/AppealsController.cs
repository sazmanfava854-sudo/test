using HRPerformance.Application.DTOs.Appeals;
using HRPerformance.Application.Features.Appeals;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class AppealsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    public AppealsController(IMediator mediator, ICurrentUserService currentUser) { _mediator = mediator; _currentUser = currentUser; }
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] AppealStatus? status) =>
        Ok(await _mediator.Send(new GetAppealsQuery(_currentUser.OrganizationId ?? Guid.Empty, status)));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateAppealRequest request) =>
        Ok(await _mediator.Send(new CreateAppealCommand(request, _currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpPut("review")] [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> Review([FromBody] ReviewAppealRequest request) =>
        Ok(await _mediator.Send(new ReviewAppealCommand(request, _currentUser.UserId ?? Guid.Empty)));
}

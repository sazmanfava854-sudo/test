using HRPerformance.Application.DTOs.Employees;
using HRPerformance.Application.Features.Employees;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    public EmployeesController(IMediator mediator, ICurrentUserService currentUser) { _mediator = mediator; _currentUser = currentUser; }
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] EmployeeSearchRequest request) =>
        Ok(await _mediator.Send(new GetEmployeesQuery(request, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpGet("{id:guid}")] public async Task<IActionResult> GetById(Guid id) => Ok(await _mediator.Send(new GetEmployeeByIdQuery(id)));
    [HttpPost] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator,Manager")] public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request) =>
        Ok(await _mediator.Send(new CreateEmployeeCommand(request, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpPut] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator,Manager")] public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest request) =>
        Ok(await _mediator.Send(new UpdateEmployeeCommand(request)));
    [HttpDelete("{id:guid}")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> Delete(Guid id) =>
        Ok(await _mediator.Send(new DeleteEmployeeCommand(id)));
}

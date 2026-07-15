using HRPerformance.Application.DTOs.Evaluations;
using HRPerformance.Application.Features.Evaluations;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")] [Authorize]
public class EvaluationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    public EvaluationsController(IMediator mediator, ICurrentUserService currentUser) { _mediator = mediator; _currentUser = currentUser; }
    [HttpGet("categories")] public async Task<IActionResult> GetCategories() =>
        Ok(await _mediator.Send(new GetCategoriesQuery(_currentUser.OrganizationId ?? Guid.Empty)));
    [HttpPost("categories")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req) =>
        Ok(await _mediator.Send(new CreateCategoryCommand(_currentUser.OrganizationId ?? Guid.Empty, req.Name, req.Description, req.Color, req.Icon, req.Weight)));
    [HttpPost] [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> Create([FromBody] CreateEvaluationRequest request) =>
        Ok(await _mediator.Send(new CreateEvaluationCommand(request, _currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty)));
    [HttpPost("rules")] [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")] public async Task<IActionResult> CreateRule([FromBody] CreateRuleRequest req) =>
        Ok(await _mediator.Send(new CreateRuleCommand(_currentUser.OrganizationId ?? Guid.Empty, req.Name, req.ConditionType, req.Operator, req.MinValue, req.MaxValue, req.ScoreImpact)));
}
public record CreateCategoryRequest(string Name, string? Description, string? Color, string? Icon, decimal Weight);
public record CreateRuleRequest(string Name, RuleConditionType ConditionType, RuleOperator Operator, decimal? MinValue, decimal? MaxValue, decimal ScoreImpact);

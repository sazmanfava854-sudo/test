using HRPerformance.DTOs.Evaluations;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EvaluationsController : ControllerBase
{
    private readonly EvaluationService _evaluationService;
    private readonly ICurrentUserService _currentUser;

    public EvaluationsController(EvaluationService evaluationService, ICurrentUserService currentUser)
    {
        _evaluationService = evaluationService;
        _currentUser = currentUser;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() =>
        Ok(await _evaluationService.GetCategoriesAsync(_currentUser.OrganizationId ?? Guid.Empty));

    [HttpPost("categories")]
    [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req) =>
        Ok(await _evaluationService.CreateCategoryAsync(_currentUser.OrganizationId ?? Guid.Empty, req.Name, req.Description, req.Color, req.Icon, req.Weight));

    [HttpPost]
    [Authorize(Roles = "Manager,OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> Create([FromBody] CreateEvaluationRequest request) =>
        Ok(await _evaluationService.CreateAsync(request, _currentUser.EmployeeId ?? Guid.Empty, _currentUser.OrganizationId ?? Guid.Empty));

    [HttpPost("rules")]
    [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> CreateRule([FromBody] CreateRuleRequest req) =>
        Ok(await _evaluationService.CreateRuleAsync(_currentUser.OrganizationId ?? Guid.Empty, req.Name, req.ConditionType, req.Operator, req.MinValue, req.MaxValue, req.ScoreImpact));
}

public record CreateCategoryRequest(string Name, string? Description, string? Color, string? Icon, decimal Weight);
public record CreateRuleRequest(string Name, RuleConditionType ConditionType, RuleOperator Operator, decimal? MinValue, decimal? MaxValue, decimal ScoreImpact);

using HRPerformance.Common;
using HRPerformance.DTOs.Evaluations;
using HRPerformance.Entities;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.App;

public class EvaluationService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public EvaluationService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<ApiResponse<EmployeeEvaluationDto>> CreateAsync(CreateEvaluationRequest request, Guid evaluatorId, Guid organizationId, CancellationToken ct = default)
    {
        var eval = new EmployeeEvaluation
        {
            EmployeeId = request.EmployeeId,
            EvaluatorId = evaluatorId,
            OrganizationId = organizationId,
            CategoryId = request.CategoryId,
            ItemId = request.ItemId,
            Score = request.Score,
            ScoreType = request.ScoreType,
            Notes = request.Notes,
            EvaluationDate = request.EvaluationDate
        };
        await _uow.Repository<EmployeeEvaluation>().AddAsync(eval, ct);
        var score = new EmployeeScore
        {
            EmployeeId = eval.EmployeeId,
            OrganizationId = organizationId,
            Score = eval.Score,
            ScoreType = eval.ScoreType,
            Title = "ارزیابی دستی",
            Description = eval.Notes,
            ScoreDate = eval.EvaluationDate,
            Year = eval.EvaluationDate.Year,
            Month = eval.EvaluationDate.Month
        };
        await _uow.Repository<EmployeeScore>().AddAsync(score, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", "EmployeeEvaluation", eval.Id.ToString(), ct: ct);
        var emp = await _uow.Repository<Employee>().GetByIdAsync(eval.EmployeeId, ct);
        return ApiResponse<EmployeeEvaluationDto>.Ok(new EmployeeEvaluationDto(eval.Id, eval.EmployeeId, emp?.FullName ?? "", eval.Score, eval.ScoreType, eval.Notes, eval.EvaluationDate, eval.WorkflowStatus));
    }

    public async Task<ApiResponse<IList<EvaluationCategoryDto>>> GetCategoriesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var cats = await _uow.Repository<EvaluationCategory>().Query().Include(c => c.Items)
            .Where(c => c.OrganizationId == organizationId && !c.IsDeleted).ToListAsync(ct);
        var dtos = cats.Select(c => new EvaluationCategoryDto(c.Id, c.Name, c.Description, c.Color, c.Icon, c.Weight, c.IsActive, c.Items.Count)).ToList();
        return ApiResponse<IList<EvaluationCategoryDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<EvaluationCategoryDto>> CreateCategoryAsync(Guid organizationId, string name, string? description, string? color, string? icon, decimal weight, CancellationToken ct = default)
    {
        var cat = new EvaluationCategory { OrganizationId = organizationId, Name = name, Description = description, Color = color, Icon = icon, Weight = weight };
        await _uow.Repository<EvaluationCategory>().AddAsync(cat, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<EvaluationCategoryDto>.Ok(new EvaluationCategoryDto(cat.Id, cat.Name, cat.Description, cat.Color, cat.Icon, cat.Weight, cat.IsActive, 0));
    }

    public async Task<ApiResponse<EvaluationRuleDto>> CreateRuleAsync(Guid organizationId, string name, RuleConditionType conditionType, RuleOperator op, decimal? minValue, decimal? maxValue, decimal scoreImpact, CancellationToken ct = default)
    {
        var rule = new EvaluationRule { OrganizationId = organizationId, Name = name, ConditionType = conditionType, Operator = op, MinValue = minValue, MaxValue = maxValue, ScoreImpact = scoreImpact };
        await _uow.Repository<EvaluationRule>().AddAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<EvaluationRuleDto>.Ok(new EvaluationRuleDto(rule.Id, rule.Name, rule.Description, rule.ConditionType, rule.Operator, rule.MinValue, rule.MaxValue, rule.ScoreImpact, rule.IsActive));
    }
}

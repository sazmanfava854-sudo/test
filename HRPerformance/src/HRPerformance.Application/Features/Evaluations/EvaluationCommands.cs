using AutoMapper;
using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Evaluations;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Evaluations;
public record CreateEvaluationCommand(CreateEvaluationRequest Request, Guid EvaluatorId, Guid OrganizationId) : IRequest<ApiResponse<EmployeeEvaluationDto>>;
public record GetCategoriesQuery(Guid OrganizationId) : IRequest<ApiResponse<IList<EvaluationCategoryDto>>>;
public record CreateCategoryCommand(Guid OrganizationId, string Name, string? Description, string? Color, string? Icon, decimal Weight) : IRequest<ApiResponse<EvaluationCategoryDto>>;
public record CreateRuleCommand(Guid OrganizationId, string Name, RuleConditionType ConditionType, RuleOperator Operator, decimal? MinValue, decimal? MaxValue, decimal ScoreImpact) : IRequest<ApiResponse<EvaluationRuleDto>>;

public class CreateEvaluationCommandHandler : IRequestHandler<CreateEvaluationCommand, ApiResponse<EmployeeEvaluationDto>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper; private readonly IAuditService _audit;
    public CreateEvaluationCommandHandler(IUnitOfWork uow, IMapper mapper, IAuditService audit) { _uow = uow; _mapper = mapper; _audit = audit; }
    public async Task<ApiResponse<EmployeeEvaluationDto>> Handle(CreateEvaluationCommand cmd, CancellationToken ct)
    {
        var eval = new EmployeeEvaluation { EmployeeId = cmd.Request.EmployeeId, EvaluatorId = cmd.EvaluatorId, OrganizationId = cmd.OrganizationId,
            CategoryId = cmd.Request.CategoryId, ItemId = cmd.Request.ItemId, Score = cmd.Request.Score, ScoreType = cmd.Request.ScoreType,
            Notes = cmd.Request.Notes, EvaluationDate = cmd.Request.EvaluationDate };
        await _uow.Repository<EmployeeEvaluation>().AddAsync(eval, ct);
        var score = new EmployeeScore { EmployeeId = eval.EmployeeId, OrganizationId = cmd.OrganizationId, Score = eval.Score, ScoreType = eval.ScoreType,
            CategoryId = eval.CategoryId, ItemId = eval.ItemId,
            Title = "ارزیابی دستی", Description = eval.Notes, ScoreDate = eval.EvaluationDate, Year = eval.EvaluationDate.Year, Month = eval.EvaluationDate.Month };
        await _uow.Repository<EmployeeScore>().AddAsync(score, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", "EmployeeEvaluation", eval.Id.ToString(), ct: ct);
        var emp = await _uow.Repository<Employee>().GetByIdAsync(eval.EmployeeId, ct);
        return ApiResponse<EmployeeEvaluationDto>.Ok(new EmployeeEvaluationDto(eval.Id, eval.EmployeeId, emp?.FullName ?? "", eval.Score, eval.ScoreType, eval.Notes, eval.EvaluationDate, eval.WorkflowStatus));
    }
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, ApiResponse<IList<EvaluationCategoryDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetCategoriesQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<IList<EvaluationCategoryDto>>> Handle(GetCategoriesQuery q, CancellationToken ct)
    {
        var cats = await _uow.Repository<EvaluationCategory>().Query().Include(c => c.Items).Where(c => c.OrganizationId == q.OrganizationId && !c.IsDeleted).ToListAsync(ct);
        var dtos = cats.Select(c => new EvaluationCategoryDto(c.Id, c.Name, c.Description, c.Color, c.Icon, c.Weight, c.IsActive, c.Items.Count)).ToList();
        return ApiResponse<IList<EvaluationCategoryDto>>.Ok(dtos);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<EvaluationCategoryDto>>
{
    private readonly IUnitOfWork _uow;
    public CreateCategoryCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<EvaluationCategoryDto>> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var cat = new EvaluationCategory { OrganizationId = cmd.OrganizationId, Name = cmd.Name, Description = cmd.Description, Color = cmd.Color, Icon = cmd.Icon, Weight = cmd.Weight };
        await _uow.Repository<EvaluationCategory>().AddAsync(cat, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<EvaluationCategoryDto>.Ok(new EvaluationCategoryDto(cat.Id, cat.Name, cat.Description, cat.Color, cat.Icon, cat.Weight, cat.IsActive, 0));
    }
}

public class CreateRuleCommandHandler : IRequestHandler<CreateRuleCommand, ApiResponse<EvaluationRuleDto>>
{
    private readonly IUnitOfWork _uow;
    public CreateRuleCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<EvaluationRuleDto>> Handle(CreateRuleCommand cmd, CancellationToken ct)
    {
        var rule = new EvaluationRule { OrganizationId = cmd.OrganizationId, Name = cmd.Name, ConditionType = cmd.ConditionType, Operator = cmd.Operator, MinValue = cmd.MinValue, MaxValue = cmd.MaxValue, ScoreImpact = cmd.ScoreImpact };
        await _uow.Repository<EvaluationRule>().AddAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<EvaluationRuleDto>.Ok(new EvaluationRuleDto(rule.Id, rule.Name, rule.Description, rule.ConditionType, rule.Operator, rule.MinValue, rule.MaxValue, rule.ScoreImpact, rule.IsActive));
    }
}

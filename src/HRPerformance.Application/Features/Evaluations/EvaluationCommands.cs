using AutoMapper;
using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Evaluations;
using HRPerformance.Application.Interfaces;
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
public record GetEmployeeIndicatorsQuery(Guid EmployeeId, Guid OrganizationId) : IRequest<ApiResponse<IList<EmployeeIndicatorDto>>>;
public record SaveEmployeeIndicatorsCommand(Guid EmployeeId, Guid OrganizationId, SaveEmployeeIndicatorsRequest Request) : IRequest<ApiResponse<IList<EmployeeIndicatorDto>>>;

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
    private readonly IEvaluationCategorySeedService _categorySeed;

    public GetCategoriesQueryHandler(IUnitOfWork uow, IEvaluationCategorySeedService categorySeed)
    {
        _uow = uow;
        _categorySeed = categorySeed;
    }

    public async Task<ApiResponse<IList<EvaluationCategoryDto>>> Handle(GetCategoriesQuery q, CancellationToken ct)
    {
        if (q.OrganizationId == Guid.Empty)
            return ApiResponse<IList<EvaluationCategoryDto>>.Fail("شناسه سازمان یافت نشد — دوباره وارد شوید");

        await _categorySeed.EnsureSeededAsync(q.OrganizationId, ct);

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

public class GetEmployeeIndicatorsQueryHandler : IRequestHandler<GetEmployeeIndicatorsQuery, ApiResponse<IList<EmployeeIndicatorDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IEvaluationCategorySeedService _categorySeed;

    public GetEmployeeIndicatorsQueryHandler(IUnitOfWork uow, IEvaluationCategorySeedService categorySeed)
    {
        _uow = uow;
        _categorySeed = categorySeed;
    }

    public async Task<ApiResponse<IList<EmployeeIndicatorDto>>> Handle(GetEmployeeIndicatorsQuery q, CancellationToken ct)
    {
        var emp = await _uow.Repository<Employee>().Query()
            .FirstOrDefaultAsync(e => e.Id == q.EmployeeId && e.OrganizationId == q.OrganizationId && !e.IsDeleted, ct);
        if (emp == null) return ApiResponse<IList<EmployeeIndicatorDto>>.Fail("کارمند یافت نشد");

        if (q.OrganizationId != Guid.Empty)
            await _categorySeed.EnsureSeededAsync(q.OrganizationId, ct);

        var categories = await _uow.Repository<EvaluationCategory>().Query()
            .Where(c => c.OrganizationId == q.OrganizationId && !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

        var overrides = await _uow.Repository<EmployeeIndicatorSetting>().Query()
            .Where(s => s.EmployeeId == q.EmployeeId && !s.IsDeleted)
            .ToListAsync(ct);

        var result = categories.Select(c =>
        {
            var setting = overrides.FirstOrDefault(o => o.CategoryId == c.Id);
            return new EmployeeIndicatorDto(
                c.Id,
                c.Name,
                c.Weight,
                setting?.Weight ?? c.Weight,
                setting?.IsActive ?? true);
        }).ToList();

        return ApiResponse<IList<EmployeeIndicatorDto>>.Ok(result);
    }
}

public class SaveEmployeeIndicatorsCommandHandler : IRequestHandler<SaveEmployeeIndicatorsCommand, ApiResponse<IList<EmployeeIndicatorDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IEvaluationCategorySeedService _categorySeed;

    public SaveEmployeeIndicatorsCommandHandler(IUnitOfWork uow, IEvaluationCategorySeedService categorySeed)
    {
        _uow = uow;
        _categorySeed = categorySeed;
    }

    public async Task<ApiResponse<IList<EmployeeIndicatorDto>>> Handle(SaveEmployeeIndicatorsCommand cmd, CancellationToken ct)
    {
        var emp = await _uow.Repository<Employee>().Query()
            .FirstOrDefaultAsync(e => e.Id == cmd.EmployeeId && e.OrganizationId == cmd.OrganizationId && !e.IsDeleted, ct);
        if (emp == null) return ApiResponse<IList<EmployeeIndicatorDto>>.Fail("کارمند یافت نشد");

        var categoryIds = await _uow.Repository<EvaluationCategory>().Query()
            .Where(c => c.OrganizationId == cmd.OrganizationId && !c.IsDeleted && c.IsActive)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var existing = await _uow.Repository<EmployeeIndicatorSetting>().Query()
            .Where(s => s.EmployeeId == cmd.EmployeeId && !s.IsDeleted)
            .ToListAsync(ct);

        foreach (var item in cmd.Request.Indicators)
        {
            if (!categoryIds.Contains(item.CategoryId)) continue;

            var row = existing.FirstOrDefault(s => s.CategoryId == item.CategoryId);
            if (row == null)
            {
                await _uow.Repository<EmployeeIndicatorSetting>().AddAsync(new EmployeeIndicatorSetting
                {
                    EmployeeId = cmd.EmployeeId,
                    CategoryId = item.CategoryId,
                    Weight = item.Weight,
                    IsActive = item.IsActive
                }, ct);
            }
            else
            {
                row.Weight = item.Weight;
                row.IsActive = item.IsActive;
                row.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _uow.SaveChangesAsync(ct);
        return await new GetEmployeeIndicatorsQueryHandler(_uow, _categorySeed).Handle(
            new GetEmployeeIndicatorsQuery(cmd.EmployeeId, cmd.OrganizationId), ct);
    }
}

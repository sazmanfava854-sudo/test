using HRPerformance.Domain.Enums;
namespace HRPerformance.Application.DTOs.Evaluations;
public record EvaluationCategoryDto(Guid Id, string Name, string? Description, string? Color, string? Icon, decimal Weight, bool IsActive, int ItemCount);
public record EvaluationItemDto(Guid Id, Guid CategoryId, string Title, string? Description, ScoreType ScoreType, decimal DefaultScore, decimal MaxScore, decimal MinScore, decimal Weight, bool IsActive);
public record EvaluationRuleDto(Guid Id, string Name, string? Description, RuleConditionType ConditionType, RuleOperator Operator, decimal? MinValue, decimal? MaxValue, decimal ScoreImpact, bool IsActive);
public record CreateEvaluationRequest(Guid EmployeeId, Guid? CategoryId, Guid? ItemId, decimal Score, ScoreType ScoreType, string? Notes, DateTime EvaluationDate);
public record EmployeeEvaluationDto(Guid Id, Guid EmployeeId, string EmployeeName, decimal Score, ScoreType ScoreType, string? Notes, DateTime EvaluationDate, WorkflowStatus WorkflowStatus);

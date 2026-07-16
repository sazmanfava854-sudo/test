using HRPerformance.Enums;
namespace HRPerformance.DTOs.Appeals;
public record AppealDto(Guid Id, Guid EmployeeId, string EmployeeName, string Reason, AppealStatus Status, DateTime CreatedAt, string? ReviewComments);
public record CreateAppealRequest(Guid? ScoreId, Guid? EvaluationId, string Reason);
public record ReviewAppealRequest(Guid AppealId, AppealStatus Status, string? ReviewComments);

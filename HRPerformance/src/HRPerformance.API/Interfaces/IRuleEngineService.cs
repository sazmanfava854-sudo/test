using HRPerformance.Entities;
using HRPerformance.Enums;

namespace HRPerformance.Interfaces;

public interface IRuleEngineService
{
    Task ProcessAttendanceAsync(AttendanceLog log, CancellationToken ct = default);
    Task ProcessHourlyLeaveAsync(AttendanceLog log, int leaveDurationMinutes, CancellationToken ct = default);
    Task<decimal> EvaluateRulesAsync(Guid organizationId, RuleConditionType type, decimal value, CancellationToken ct = default);
}

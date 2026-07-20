using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;

namespace HRPerformance.Domain.Interfaces;

public interface IRuleEngineService
{
    Task ProcessAttendanceAsync(AttendanceLog log, CancellationToken ct = default);
    Task ProcessHourlyLeaveAsync(AttendanceLog log, int leaveDurationMinutes, CancellationToken ct = default);
    Task ProcessMisHourlyLeaveAsync(AttendanceLog log, int leaveDurationMinutes, CancellationToken ct = default);
    Task<decimal> EvaluateRulesAsync(Guid organizationId, RuleConditionType type, decimal value, CancellationToken ct = default);
}

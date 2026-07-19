namespace HRPerformance.Domain.Models;

public record MisSyncDateRangeRequest(
    DateTime FromDate,
    DateTime ToDate,
    int EmployeeLimit = 0);

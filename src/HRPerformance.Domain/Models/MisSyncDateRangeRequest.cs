namespace HRPerformance.Domain.Models;

public record MisSyncDateRangeRequest(
    int ShamsiFromYear,
    int ShamsiFromMonth,
    int ShamsiFromDay,
    int ShamsiToYear,
    int ShamsiToMonth,
    int ShamsiToDay,
    int EmployeeLimit = 0);

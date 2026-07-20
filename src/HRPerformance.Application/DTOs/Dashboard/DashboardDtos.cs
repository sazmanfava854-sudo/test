namespace HRPerformance.Application.DTOs.Dashboard;
public record EmployeeDashboardDto(decimal CurrentScore, decimal MonthlyScore, decimal YearlyScore, int? Ranking,
    IList<ScoreTrendDto> ScoreTrend, IList<AttendanceSummaryDto> RecentAttendance, int PositiveCount, int NegativeCount);
public record ManagerDashboardDto(int EmployeeCount, int TodayPresent, int TodayDelays, int TodayAbsent, decimal AverageScore,
    IList<TopEmployeeDto> TopEmployees, IList<TopEmployeeDto> WeakEmployees, IList<ChartDataDto> MonthlyTrend,
    IList<ChartDataDto> TeamIndicators);
public record AdminDashboardDto(int TotalEmployees, int TotalManagers, int TotalDepartments, int TodayPresent, int TodayAbsent,
    decimal AverageScore, IList<DepartmentRankDto> DepartmentRankings, IList<ChartDataDto> PerformanceDistribution);
public record ScoreTrendDto(string Label, decimal Score);
public record AttendanceSummaryDto(DateTime Date, bool IsPresent, int DelayMinutes, bool IsAbsent);
public record TopEmployeeDto(Guid Id, string FullName, string? Department, decimal Score, int? Ranking);
public record ChartDataDto(string Label, decimal Value);
public record DepartmentRankDto(Guid Id, string Name, decimal AverageScore, int EmployeeCount);
public record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    string PersonnelCode,
    string FullName,
    DateTime AttendanceDate,
    string? EntryTime,
    string? ExitTime,
    decimal? WorkingHours,
    int DelayMinutes,
    bool IsOnLeave,
    string? LeaveType,
    string Source);

namespace HRPerformance.DTOs.Reports;
public record EmployeeReportRequest(Guid EmployeeId, DateTime StartDate, DateTime EndDate);
public record DepartmentReportRequest(Guid DepartmentId, DateTime StartDate, DateTime EndDate);
public record ReportSummaryDto(string Title, DateTime GeneratedAt, object Data);

namespace HRPerformance.Domain.Models;

public record MisSyncDateRangeRequest(
    DateTime FromDate,
    DateTime ToDate,
    string ProvinceCode = "147",
    string ShamsiYearPrefix = "1404",
    bool ApplyProvinceFilter = true,
    bool ApplyShamsiYearFilter = true,
    int EmployeeLimit = 0);

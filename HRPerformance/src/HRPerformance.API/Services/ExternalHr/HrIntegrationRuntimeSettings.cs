namespace HRPerformance.Services.ExternalHr;

public class HrIntegrationRuntimeSettings
{
    public bool IsConnectionConfigured { get; init; }
    public string SourceType { get; init; } = "SQLView";
    public string ShamsiYearPrefix { get; init; } = "1404";
    public string ProvinceCode { get; init; } = "147";
    public bool ApplyProvinceFilter { get; init; } = true;
    public bool ApplyShamsiYearFilter { get; init; } = true;
    public int EmployeeLimit { get; init; }
    public string? MisConnectionString { get; init; }
}

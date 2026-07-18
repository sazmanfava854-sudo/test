namespace HRPerformance.Services.ExternalHr;

public class HrIntegrationRuntimeSettings
{
    public bool IsConnectionConfigured { get; init; }
    public string SourceType { get; init; } = "SQLView";
    public string SyncMode { get; init; } = "Monthly";
    public string ShamsiYearPrefix { get; init; } = "1404";
    public string ProvinceCode { get; init; } = "147";
    public bool ApplyProvinceFilter { get; init; } = true;
    public bool ApplyShamsiYearFilter { get; init; } = true;
    public int InitialSyncMonthsBack { get; init; } = 12;
    public int MonthsPerSyncRun { get; init; } = 1;
    public int SyncDaysBack { get; init; } = 30;
    public int EmployeeLimit { get; init; }
    public bool BackgroundSyncEnabled { get; init; }
    public int SyncIntervalMinutes { get; init; } = 5;
    public int SyncStartupDelaySeconds { get; init; } = 15;
    public DateTime? SyncFromDate { get; init; }
    public DateTime? SyncToDate { get; init; }
    public string? MisConnectionString { get; init; }
}

using HRPerformance.Common;
using HRPerformance.Enums;

namespace HRPerformance.Entities;

public class AttendanceIntegrationSetting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string SourceType { get; set; } = "REST";
    public string? EndpointUrl { get; set; }
    public string? ConnectionString { get; set; }
    public string? SqlViewName { get; set; }
    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string SyncMode { get; set; } = "Monthly";
    public string ShamsiYearPrefix { get; set; } = "1404";
    public string ProvinceCode { get; set; } = "147";
    public bool ApplyProvinceFilter { get; set; } = true;
    public bool ApplyShamsiYearFilter { get; set; } = true;
    public int InitialSyncMonthsBack { get; set; } = 12;
    public int MonthsPerSyncRun { get; set; } = 1;
    public int SyncDaysBack { get; set; } = 30;
    public int EmployeeLimit { get; set; }
    public bool BackgroundSyncEnabled { get; set; }
    public int SyncIntervalMinutes { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

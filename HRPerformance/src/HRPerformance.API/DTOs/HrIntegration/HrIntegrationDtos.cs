namespace HRPerformance.DTOs.HrIntegration;

public record HrIntegrationSettingsDto(
    Guid? Id,
    string SourceType,
    string SyncMode,
    string ShamsiYearPrefix,
    string ProvinceCode,
    bool ApplyProvinceFilter,
    bool ApplyShamsiYearFilter,
    int InitialSyncMonthsBack,
    int MonthsPerSyncRun,
    int SyncDaysBack,
    int EmployeeLimit,
    bool BackgroundSyncEnabled,
    int SyncIntervalMinutes,
    bool IsConnectionConfigured,
    DateTime? LastSyncAt);

public record UpdateHrIntegrationSettingsRequest(
    string SyncMode,
    string ShamsiYearPrefix,
    string ProvinceCode,
    bool ApplyProvinceFilter,
    bool ApplyShamsiYearFilter,
    int InitialSyncMonthsBack,
    int MonthsPerSyncRun,
    int SyncDaysBack,
    int EmployeeLimit,
    bool BackgroundSyncEnabled,
    int SyncIntervalMinutes);

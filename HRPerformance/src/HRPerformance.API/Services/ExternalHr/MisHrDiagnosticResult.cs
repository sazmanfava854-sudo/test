namespace HRPerformance.Services.ExternalHr;

public class MisHrDiagnosticResult
{
    public bool CanConnect { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SyncFrom { get; set; }
    public int SyncDaysBack { get; set; }
    public string ProvinceCode { get; set; } = string.Empty;
    public string ShamsiYearPrefix { get; set; } = string.Empty;
    public bool ApplyProvinceFilter { get; set; }
    public bool ApplyShamsiYearFilter { get; set; }
    public int TotalInView { get; set; }
    public int CountAfterSyncFrom { get; set; }
    public int? CountAfterProvince { get; set; }
    public int? CountAfterShamsiYear { get; set; }
    public int CountWithActiveFilters { get; set; }
    public int DistinctEmployeesWithActiveFilters { get; set; }
    public int EmployeesInHrDatabase { get; set; }
}

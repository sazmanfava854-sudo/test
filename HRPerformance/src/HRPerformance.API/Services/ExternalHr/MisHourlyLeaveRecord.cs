namespace HRPerformance.Services.ExternalHr;

/// <summary>
/// Record from MIS.dbo.HZG_View_HourlyLeave
/// </summary>
public class MisHourlyLeaveRecord
{
    public decimal Id { get; set; }
    public string? Code { get; set; }
    public string PerCod { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? NationalIDNo { get; set; }
    public string? ProvinceCode { get; set; }
    public int? ShamsiDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int LeaveDurationMinutes { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int FirstTimeType { get; set; }
}

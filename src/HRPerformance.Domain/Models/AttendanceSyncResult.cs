namespace HRPerformance.Domain.Models;

public class AttendanceSyncResult
{
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    /// <summary>تعداد ردیف برگشتی از MIS قبل از ذخیره</summary>
    public int MisRowsFetched { get; set; }
    /// <summary>تعداد پرسنل متمایز در MIS (CTE فیلترشده)</summary>
    public int DistinctEmployeesInMis { get; set; }
    /// <summary>کارمندان جدید ثبت‌شده در HRPerformanceDB</summary>
    public int EmployeesUpserted { get; set; }
    public IReadOnlyList<string> SyncedRanges { get; set; } = Array.Empty<string>();
}

namespace HRPerformance.Domain.Models;

public class AttendanceSyncResult
{
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    /// <summary>تعداد ردیف برگشتی از MIS قبل از ذخیره</summary>
    public int MisRowsFetched { get; set; }
    public IReadOnlyList<string> SyncedRanges { get; set; } = Array.Empty<string>();
}

namespace HRPerformance.Domain.Models;

public class AttendanceSyncResult
{
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public IReadOnlyList<string> SyncedRanges { get; set; } = Array.Empty<string>();
}

namespace HRPerformance.Services;

public class AttendanceSyncResult
{
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public IReadOnlyList<string> SyncedRanges { get; set; } = Array.Empty<string>();
    public bool IsBackfillComplete { get; set; }
    public string? NextRangeDescription { get; set; }
}

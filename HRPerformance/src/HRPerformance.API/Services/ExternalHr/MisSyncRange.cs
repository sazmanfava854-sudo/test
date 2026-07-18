namespace HRPerformance.Services.ExternalHr;

public class MisSyncRange
{
    public DateTime SyncFrom { get; init; }
    public DateTime SyncToExclusive { get; init; }
    public int? ShamsiYear { get; init; }
    public int? ShamsiMonth { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsBackfill { get; init; }
}

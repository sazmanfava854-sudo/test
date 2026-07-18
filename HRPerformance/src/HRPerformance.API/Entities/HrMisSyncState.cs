namespace HRPerformance.Entities;

public class HrMisSyncState
{
    public Guid OrganizationId { get; set; }
    public int TargetShamsiYear { get; set; }
    public int NextShamsiMonth { get; set; }
    public int BackfillStartMonth { get; set; } = 1;
    public bool IsBackfillComplete { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? LastSyncDescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

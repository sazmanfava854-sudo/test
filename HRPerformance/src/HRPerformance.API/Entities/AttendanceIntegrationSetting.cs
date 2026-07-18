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
    public int SyncIntervalMinutes { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

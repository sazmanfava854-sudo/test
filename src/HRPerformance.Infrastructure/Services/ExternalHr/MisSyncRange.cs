namespace HRPerformance.Infrastructure.Services.ExternalHr;

public class MisSyncRange
{
    /// <summary>14040410 برای 1404/04/10</summary>
    public int ShamsiFromKey { get; init; }

    /// <summary>14040411 برای 1404/04/11</summary>
    public int ShamsiToKey { get; init; }

    public string ShamsiFromText { get; init; } = string.Empty;
    public string ShamsiToText { get; init; } = string.Empty;
    public int? ShamsiFromYm { get; init; }
    public int? ShamsiToYm { get; init; }
    public int? ShamsiYear { get; init; }
    public int? ShamsiMonth { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsBackfill { get; init; }
}

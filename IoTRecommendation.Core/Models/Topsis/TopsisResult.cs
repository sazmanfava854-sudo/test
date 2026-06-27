namespace IoTRecommendation.Core.Models.Topsis;

/// <summary>
/// Full output of Phase 4 TOPSIS, restricted to the user-selected cluster.
/// </summary>
public sealed class TopsisResult
{
    public List<TopsisRankEntry> Ranking { get; set; } = new();

    /// <summary>Adaptive weights used in this TOPSIS run.</summary>
    public Dictionary<string, double> WeightsUsed { get; set; } = new();

    public int SelectedClusterId { get; set; }
    public string WinnerName { get; set; } = string.Empty;
}

namespace IoTRecommendation.Core.Models.Copras;

/// <summary>
/// Full output of the COPRAS algorithm, restricted to the user-selected cluster.
/// Alternatives are ranked by descending Qᵢ (higher Qᵢ = better).
/// </summary>
public sealed class CoprasResult
{
    public List<CoprasRankEntry> Ranking { get; set; } = new();

    /// <summary>Adaptive weights used in this COPRAS run.</summary>
    public Dictionary<string, double> WeightsUsed { get; set; } = new();

    public int SelectedClusterId { get; set; }
    public string WinnerName { get; set; } = string.Empty;
}

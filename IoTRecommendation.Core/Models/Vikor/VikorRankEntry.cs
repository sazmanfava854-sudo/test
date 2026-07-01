namespace IoTRecommendation.Core.Models.Vikor;

/// <summary>
/// VIKOR result for one technology.
/// </summary>
public sealed class VikorRankEntry
{
    public int Rank { get; set; }
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;

    /// <summary>Utility measure — weighted sum of normalised distances from the best value (lower = better).</summary>
    public double S { get; set; }

    /// <summary>Regret measure — maximum weighted normalised distance from the best value (lower = better).</summary>
    public double R { get; set; }

    /// <summary>VIKOR index. Lower Q means closer to the ideal compromise solution.</summary>
    public double Q { get; set; }
}

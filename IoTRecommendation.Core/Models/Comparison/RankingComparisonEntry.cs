namespace IoTRecommendation.Core.Models.Comparison;

/// <summary>
/// Side-by-side comparison of one technology's rank/score under TOPSIS and VIKOR.
/// </summary>
public sealed class RankingComparisonEntry
{
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;

    public int TopsisRank { get; set; }
    public double TopsisCloseness { get; set; }

    public int VikorRank { get; set; }
    public double VikorQ { get; set; }

    /// <summary>TopsisRank − VikorRank. Zero means both methods agree on this position.</summary>
    public int RankDifference => TopsisRank - VikorRank;
}

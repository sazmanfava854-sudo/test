namespace IoTRecommendation.Core.Models.Comparison;

/// <summary>
/// Side-by-side comparison of one technology's rank/score under all three
/// MCDM methods (TOPSIS, VIKOR, COPRAS).
/// </summary>
public sealed class MultiMethodComparisonEntry
{
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;

    public int TopsisRank { get; set; }
    public double TopsisScore { get; set; }

    public int VikorRank { get; set; }
    public double VikorScore { get; set; }

    public int CoprasRank { get; set; }
    public double CoprasScore { get; set; }

    /// <summary>True when TOPSIS, VIKOR, and COPRAS all agree on this technology's rank position.</summary>
    public bool AllRanksAgree => TopsisRank == VikorRank && VikorRank == CoprasRank;
}

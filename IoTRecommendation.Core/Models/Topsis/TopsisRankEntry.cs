namespace IoTRecommendation.Core.Models.Topsis;

/// <summary>
/// TOPSIS result for one technology.
/// </summary>
public sealed class TopsisRankEntry
{
    public int Rank { get; set; }
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;
    public double DistanceToBestIdeal { get; set; }
    public double DistanceToWorstIdeal { get; set; }
    public double ClosenessCoefficient { get; set; }
}

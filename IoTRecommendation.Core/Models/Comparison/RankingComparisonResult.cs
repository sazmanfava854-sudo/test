namespace IoTRecommendation.Core.Models.Comparison;

/// <summary>
/// Full comparison of TOPSIS and VIKOR rankings computed from the same cluster,
/// technology dataset, and adaptive weights.
/// </summary>
public sealed class RankingComparisonResult
{
    /// <summary>One entry per technology, ordered by TOPSIS rank.</summary>
    public List<RankingComparisonEntry> Entries { get; set; } = new();

    public string TopsisWinner { get; set; } = string.Empty;
    public string VikorWinner { get; set; } = string.Empty;

    /// <summary>True when both methods select the same top-ranked technology.</summary>
    public bool WinnersAgree { get; set; }

    /// <summary>
    /// Spearman's rank correlation coefficient (ρ) between the two full rankings.
    /// Close to +1 means the methods agree strongly; close to 0 or negative
    /// means they disagree — useful evidence for a robustness/sensitivity discussion.
    /// </summary>
    public double SpearmanRankCorrelation { get; set; }
}

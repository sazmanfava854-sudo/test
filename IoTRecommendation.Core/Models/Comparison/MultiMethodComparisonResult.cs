namespace IoTRecommendation.Core.Models.Comparison;

/// <summary>
/// Unified, JSON-serialisable comparison of TOPSIS, VIKOR, and COPRAS —
/// final scores, ranks per technology, each method's winner, and all three
/// pairwise Spearman rank-correlation coefficients. This is the single
/// combined output requested for side-by-side display/consumption.
/// </summary>
public sealed class MultiMethodComparisonResult
{
    /// <summary>One entry per technology, ordered by TOPSIS rank.</summary>
    public List<MultiMethodComparisonEntry> Entries { get; set; } = new();

    public string TopsisWinner { get; set; } = string.Empty;
    public string VikorWinner { get; set; } = string.Empty;
    public string CoprasWinner { get; set; } = string.Empty;

    /// <summary>True when all three methods select the same top-ranked technology.</summary>
    public bool AllWinnersAgree { get; set; }

    /// <summary>Spearman's ρ between the TOPSIS and VIKOR rankings.</summary>
    public double SpearmanTopsisVikor { get; set; }

    /// <summary>Spearman's ρ between the TOPSIS and COPRAS rankings.</summary>
    public double SpearmanTopsisCopras { get; set; }

    /// <summary>Spearman's ρ between the VIKOR and COPRAS rankings.</summary>
    public double SpearmanVikorCopras { get; set; }
}

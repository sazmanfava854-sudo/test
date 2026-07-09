namespace IoTRecommendation.Core.Models.Copras;

/// <summary>
/// COPRAS result for one technology.
/// </summary>
public sealed class CoprasRankEntry
{
    public int Rank { get; set; }
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;

    /// <summary>Sum of weighted normalised values over benefit (maximising) criteria.</summary>
    public double SPlus { get; set; }

    /// <summary>Sum of weighted normalised values over cost (minimising) criteria.</summary>
    public double SMinus { get; set; }

    /// <summary>
    /// Relative significance (priority) Qᵢ. Unlike VIKOR's Q, a HIGHER Qᵢ is
    /// better here — alternatives are ranked by Qᵢ descending.
    /// </summary>
    public double Q { get; set; }

    /// <summary>Utility degree Nᵢ (%) — Qᵢ relative to the best (maximum) Qᵢ, in [0, 100].</summary>
    public double N { get; set; }
}

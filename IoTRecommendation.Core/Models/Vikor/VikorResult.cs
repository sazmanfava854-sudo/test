namespace IoTRecommendation.Core.Models.Vikor;

/// <summary>
/// Full output of the VIKOR algorithm, restricted to the user-selected cluster.
/// Alternatives are ranked by ascending Q (lower Q = closer to the ideal compromise).
/// </summary>
public sealed class VikorResult
{
    public List<VikorRankEntry> Ranking { get; set; } = new();

    /// <summary>Weight of the "majority of criteria" strategy (0..1). 0.5 is the classical default.</summary>
    public double V { get; set; }

    /// <summary>Adaptive weights used in this VIKOR run.</summary>
    public Dictionary<string, double> WeightsUsed { get; set; } = new();

    public int SelectedClusterId { get; set; }
    public string WinnerName { get; set; } = string.Empty;

    /// <summary>
    /// Condition C1 (acceptable advantage): Q(a2) − Q(a1) ≥ DQ, where DQ = 1/(m−1).
    /// </summary>
    public bool AcceptableAdvantage { get; set; }

    /// <summary>
    /// Condition C2 (acceptable stability): the top-ranked alternative by Q must
    /// also be the best alternative under S alone or under R alone.
    /// </summary>
    public bool AcceptableStability { get; set; }

    /// <summary>True only when both C1 and C2 hold — a single, statistically robust winner.</summary>
    public bool IsCompromiseSolutionStable => AcceptableAdvantage && AcceptableStability;

    /// <summary>
    /// Technology ids proposed as compromise solutions. Contains only the winner
    /// when both conditions hold; otherwise contains every alternative within DQ
    /// of the best Q (per Opricovic &amp; Tzeng, 2004; 2007).
    /// </summary>
    public List<string> CompromiseSet { get; set; } = new();
}

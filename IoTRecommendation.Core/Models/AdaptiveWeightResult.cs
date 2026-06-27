namespace IoTRecommendation.Core.Models;

/// <summary>
/// Output of Phase 3: adaptive weight adjustment.
/// Contains base weights, per-criterion adjustment scores (Si),
/// and the final normalised adaptive weights.
/// </summary>
public sealed class AdaptiveWeightResult
{
    /// <summary>Base AHP weights before adjustment.</summary>
    public Dictionary<string, double> BaseWeights { get; set; } = new();

    /// <summary>
    /// Net adjustment score per criterion.
    /// Si = PositiveCount − NegativeCount across all answered questions.
    /// </summary>
    public Dictionary<string, int> AdjustmentScores { get; set; } = new();

    /// <summary>Positive effect count per criterion.</summary>
    public Dictionary<string, int> PositiveCounts { get; set; } = new();

    /// <summary>Negative effect count per criterion.</summary>
    public Dictionary<string, int> NegativeCounts { get; set; } = new();

    /// <summary>Final normalised weights after adaptive formula.</summary>
    public Dictionary<string, double> AdaptiveWeights { get; set; } = new();
}

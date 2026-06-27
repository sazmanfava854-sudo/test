namespace IoTRecommendation.Core.Models;

/// <summary>
/// Pairwise Comparison Matrix (PCM) provided by one domain expert.
/// Loaded from Experts/ExpertN.json. The matrix must be square and
/// reciprocal (matrix[i][j] == 1 / matrix[j][i]).
/// </summary>
public sealed class Expert
{
    public string ExpertId { get; set; } = string.Empty;
    public string ExpertName { get; set; } = string.Empty;

    /// <summary>
    /// Row-major PCM. Indices must correspond to the criteria order
    /// defined in Settings.json (only criteria with UsedInTopsis = true).
    /// </summary>
    public double[][] Matrix { get; set; } = [];
}

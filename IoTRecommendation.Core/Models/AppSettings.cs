namespace IoTRecommendation.Core.Models;

/// <summary>
/// Application-wide settings loaded from Settings.json.
/// All tunable parameters live here so nothing is hardcoded.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Maximum acceptable Consistency Ratio for AHP matrices.</summary>
    public double CrAcceptableThreshold { get; set; } = 0.10;

    /// <summary>
    /// Sensitivity parameter α for the exponential adaptive weight formula.
    /// W'_i = W_i * exp(α * Si)  (then normalised).
    /// Increase α to amplify questionnaire effects.
    /// </summary>
    public double AdaptiveWeightAlpha { get; set; } = 0.5;

    public int KMeansMin { get; set; } = 2;
    public int KMeansMax { get; set; } = 6;
    public int KMeansRandomSeed { get; set; } = 42;
    public int KMeansMaxIterations { get; set; } = 300;
    public int KMeansRestarts { get; set; } = 10;

    /// <summary>Ordered list of all decision criteria (AHP + TOPSIS order).</summary>
    public List<CriterionDefinition> CriteriaDefinitions { get; set; } = new();
}

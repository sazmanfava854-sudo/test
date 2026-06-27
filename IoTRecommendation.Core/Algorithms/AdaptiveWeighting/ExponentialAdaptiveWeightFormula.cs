using IoTRecommendation.Core.Interfaces;

namespace IoTRecommendation.Core.Algorithms.AdaptiveWeighting;

/// <summary>
/// Default adaptive weight formula using exponential scaling.
///
/// Formula:   W'_i  = W_i * exp(α * Si)
/// Then:      W_i   = W'_i / Σ W'_j   (normalise to sum = 1)
///
/// Properties:
///   • Always produces positive weights regardless of Si sign.
///   • α = 0 → identical to base AHP weights (no adaptation).
///   • Larger |Si| → stronger deviation from base weights.
///   • α is loaded from Settings.json (never hardcoded here).
///
/// Replacement: create a new class implementing IAdaptiveWeightFormula
/// and swap it in DI — this class does not need to change.
/// </summary>
public sealed class ExponentialAdaptiveWeightFormula : IAdaptiveWeightFormula
{
    private readonly double _alpha;

    public ExponentialAdaptiveWeightFormula(double alpha)
    {
        if (alpha < 0) throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be non-negative.");
        _alpha = alpha;
    }

    public Dictionary<string, double> Calculate(
        IReadOnlyDictionary<string, double> baseWeights,
        IReadOnlyDictionary<string, int> adjustmentScores)
    {
        var adjusted = baseWeights.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value * Math.Exp(_alpha * adjustmentScores.GetValueOrDefault(kvp.Key, 0)));

        double sum = adjusted.Values.Sum();
        if (sum <= 0 || !double.IsFinite(sum))
            return baseWeights.ToDictionary(k => k.Key, k => k.Value);

        return adjusted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / sum);
    }
}

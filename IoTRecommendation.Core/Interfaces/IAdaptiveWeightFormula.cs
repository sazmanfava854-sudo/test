namespace IoTRecommendation.Core.Interfaces;

/// <summary>
/// Strategy interface for the adaptive weight formula in Phase 3.
///
/// The formula receives base AHP weights and per-criterion adjustment
/// scores (Si = PositiveCount − NegativeCount) and returns new
/// normalised weights.
///
/// To replace the formula (e.g. change from exponential to linear):
///   1. Implement this interface in a new class.
///   2. Register the new class in DI instead of the current implementation.
///   Nothing else changes.
/// </summary>
public interface IAdaptiveWeightFormula
{
    /// <summary>
    /// Computes adaptive weights.
    /// </summary>
    /// <param name="baseWeights">Normalised AHP weights (sum ≈ 1), keyed by criterion key.</param>
    /// <param name="adjustmentScores">Si value per criterion key.</param>
    /// <returns>New normalised weights (sum = 1), keyed by criterion key.</returns>
    Dictionary<string, double> Calculate(
        IReadOnlyDictionary<string, double> baseWeights,
        IReadOnlyDictionary<string, int> adjustmentScores);
}

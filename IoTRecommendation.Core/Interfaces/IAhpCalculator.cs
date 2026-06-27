using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Ahp;

namespace IoTRecommendation.Core.Interfaces;

/// <summary>
/// Computes AHP priority vectors and consistency measures.
/// </summary>
public interface IAhpCalculator
{
    /// <summary>
    /// Aggregates multiple expert PCMs via geometric mean, then computes
    /// priority vector, λmax, CI, RI, CR for the aggregated matrix.
    /// </summary>
    AhpResult Calculate(IReadOnlyList<Expert> experts, IReadOnlyList<CriterionDefinition> criteria);

    /// <summary>
    /// Convenience overload to compute AHP on a single (already aggregated) matrix.
    /// </summary>
    AhpResult CalculateFromMatrix(double[][] matrix, IReadOnlyList<CriterionDefinition> criteria);
}

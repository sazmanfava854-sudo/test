namespace IoTRecommendation.Core.Models.Ahp;

/// <summary>
/// Full output of one AHP calculation run.
/// Contains priority vector, consistency measures, and per-expert details.
/// </summary>
public sealed class AhpResult
{
    /// <summary>Normalised priority weights keyed by CriterionDefinition.Key.</summary>
    public Dictionary<string, double> Weights { get; set; } = new();

    /// <summary>Dominant eigenvalue of the aggregated PCM.</summary>
    public double LambdaMax { get; set; }

    /// <summary>Consistency Index = (λmax − n) / (n − 1).</summary>
    public double ConsistencyIndex { get; set; }

    /// <summary>Random Index from Saaty's lookup table.</summary>
    public double RandomIndex { get; set; }

    /// <summary>Consistency Ratio = CI / RI.</summary>
    public double ConsistencyRatio { get; set; }

    /// <summary>True when CR ≤ 0.10.</summary>
    public bool IsConsistent { get; set; }

    /// <summary>Per-expert CR values for transparency.</summary>
    public List<ExpertConsistency> ExpertConsistencies { get; set; } = new();

    /// <summary>Aggregated PCM (geometric mean of all experts), row-major.</summary>
    public double[][] AggregatedMatrix { get; set; } = [];
}

public sealed class ExpertConsistency
{
    public string ExpertId { get; set; } = string.Empty;
    public string ExpertName { get; set; } = string.Empty;
    public double ConsistencyRatio { get; set; }
    public bool IsConsistent { get; set; }
}

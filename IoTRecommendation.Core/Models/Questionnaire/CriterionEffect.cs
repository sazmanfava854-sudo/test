namespace IoTRecommendation.Core.Models.Questionnaire;

/// <summary>
/// A single (+1 / -1 / 0) effect that one question-option has on a criterion.
/// </summary>
public sealed class CriterionEffect
{
    /// <summary>Key matching CriterionDefinition.Key.</summary>
    public string CriterionKey { get; set; } = string.Empty;

    /// <summary>+1 increases importance, -1 decreases importance, 0 has no effect.</summary>
    public int Delta { get; set; }
}

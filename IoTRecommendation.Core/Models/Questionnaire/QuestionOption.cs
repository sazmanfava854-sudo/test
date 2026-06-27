namespace IoTRecommendation.Core.Models.Questionnaire;

/// <summary>
/// One selectable answer for a question.
/// </summary>
public sealed class QuestionOption
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Criterion effects triggered when the user selects this option.
    /// Empty list means "no effect".
    /// </summary>
    public List<CriterionEffect> Effects { get; set; } = new();
}

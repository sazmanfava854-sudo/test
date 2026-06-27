namespace IoTRecommendation.Core.Models.Questionnaire;

/// <summary>
/// Records the user's answer to a single question.
/// </summary>
public sealed class QuestionnaireAnswer
{
    public string QuestionId { get; set; } = string.Empty;
    public string SelectedOptionId { get; set; } = string.Empty;
}

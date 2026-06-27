namespace IoTRecommendation.Core.Models.Questionnaire;

/// <summary>
/// One question in the adaptive-weighting questionnaire.
/// Loaded from Questions.json — never hardcoded.
/// </summary>
public sealed class Question
{
    public string Id { get; set; } = string.Empty;
    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<QuestionOption> Options { get; set; } = new();
}

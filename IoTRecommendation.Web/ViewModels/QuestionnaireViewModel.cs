using IoTRecommendation.Core.Models.Questionnaire;

namespace IoTRecommendation.Web.ViewModels;

public sealed class QuestionnaireViewModel
{
    public List<Question> Questions { get; set; } = new();

    /// <summary>Flat dictionary of questionId → selected optionId, used for form binding.</summary>
    public Dictionary<string, string> SelectedOptions { get; set; } = new();
}

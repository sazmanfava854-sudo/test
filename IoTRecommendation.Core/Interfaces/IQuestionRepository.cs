using IoTRecommendation.Core.Models.Questionnaire;

namespace IoTRecommendation.Core.Interfaces;

public interface IQuestionRepository
{
    Task<IReadOnlyList<Question>> GetAllAsync();
}

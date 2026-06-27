using IoTRecommendation.Core.Models;

namespace IoTRecommendation.Core.Interfaces;

public interface IExpertRepository
{
    Task<IReadOnlyList<Expert>> GetAllAsync();
}

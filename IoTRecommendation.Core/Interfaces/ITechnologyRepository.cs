using IoTRecommendation.Core.Models;

namespace IoTRecommendation.Core.Interfaces;

public interface ITechnologyRepository
{
    Task<IReadOnlyList<Technology>> GetAllAsync();
}

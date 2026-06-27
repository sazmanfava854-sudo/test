using IoTRecommendation.Core.Models;

namespace IoTRecommendation.Core.Interfaces;

public interface ISettingsRepository
{
    Task<AppSettings> GetAsync();
}

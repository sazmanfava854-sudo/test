using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models.Ahp;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates Phase 2: loads all expert matrices, runs AHP, and
/// returns base weights with full consistency reporting.
/// </summary>
public sealed class AhpService
{
    private readonly IExpertRepository _expertRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAhpCalculator _ahpCalculator;

    public AhpService(
        IExpertRepository expertRepository,
        ISettingsRepository settingsRepository,
        IAhpCalculator ahpCalculator)
    {
        _expertRepository = expertRepository;
        _settingsRepository = settingsRepository;
        _ahpCalculator = ahpCalculator;
    }

    public async Task<AhpResult> RunAsync()
    {
        var experts = await _expertRepository.GetAllAsync();
        var settings = await _settingsRepository.GetAsync();

        if (experts.Count == 0)
            throw new InvalidOperationException(
                "No expert matrices found. Add at least one file in Data/Experts/.");

        return _ahpCalculator.Calculate(experts, settings.CriteriaDefinitions);
    }
}

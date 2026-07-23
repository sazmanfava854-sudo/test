using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Infrastructure.Configuration;
using IoTRecommendation.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoTRecommendation.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure services (repositories, configuration).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DataPathOptions>(configuration.GetSection(DataPathOptions.SectionName));

        services.AddSingleton<ITechnologyRepository, JsonTechnologyRepository>();
        services.AddSingleton<IExpertRepository, JsonExpertRepository>();
        services.AddSingleton<IQuestionRepository, JsonQuestionRepository>();
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();
        services.AddSingleton<IClusterTaxonomyRepository, JsonClusterTaxonomyRepository>();

        return services;
    }
}

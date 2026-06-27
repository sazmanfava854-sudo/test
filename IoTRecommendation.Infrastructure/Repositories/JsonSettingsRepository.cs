using System.Text.Json;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IoTRecommendation.Infrastructure.Repositories;

public sealed class JsonSettingsRepository : ISettingsRepository
{
    private readonly DataPathOptions _paths;
    private AppSettings? _cache;

    public JsonSettingsRepository(IOptions<DataPathOptions> options) => _paths = options.Value;

    public async Task<AppSettings> GetAsync()
    {
        if (_cache is not null) return _cache;

        string filePath = Path.Combine(_paths.DataDirectory, _paths.SettingsFile);
        await using var stream = File.OpenRead(filePath);
        _cache = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions.Default)
                 ?? throw new InvalidOperationException($"Failed to deserialize {filePath}");
        return _cache;
    }
}

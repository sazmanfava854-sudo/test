using System.Text.Json;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IoTRecommendation.Infrastructure.Repositories;

public sealed class JsonTechnologyRepository : ITechnologyRepository
{
    private readonly DataPathOptions _paths;
    private IReadOnlyList<Technology>? _cache;

    public JsonTechnologyRepository(IOptions<DataPathOptions> options) => _paths = options.Value;

    public async Task<IReadOnlyList<Technology>> GetAllAsync()
    {
        if (_cache is not null) return _cache;

        string filePath = Path.Combine(_paths.DataDirectory, _paths.TechnologiesFile);
        await using var stream = File.OpenRead(filePath);
        var list = await JsonSerializer.DeserializeAsync<List<Technology>>(stream, JsonOptions.Default)
                   ?? throw new InvalidOperationException($"Failed to deserialize {filePath}");
        _cache = list.AsReadOnly();
        return _cache;
    }
}

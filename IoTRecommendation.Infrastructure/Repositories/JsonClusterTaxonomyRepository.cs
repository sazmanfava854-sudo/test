using System.Text.Json;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models.Clustering;
using IoTRecommendation.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IoTRecommendation.Infrastructure.Repositories;

public sealed class JsonClusterTaxonomyRepository : IClusterTaxonomyRepository
{
    private readonly DataPathOptions _paths;
    private IReadOnlyList<ClusterTaxonomyEntry>? _cache;

    public JsonClusterTaxonomyRepository(IOptions<DataPathOptions> options) => _paths = options.Value;

    public async Task<IReadOnlyList<ClusterTaxonomyEntry>> GetAsync()
    {
        if (_cache is not null) return _cache;

        string path = Path.Combine(_paths.DataDirectory, _paths.ClusterTaxonomyFile);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Cluster taxonomy file not found: {path}");

        await using var stream = File.OpenRead(path);
        var list = await JsonSerializer.DeserializeAsync<List<ClusterTaxonomyEntry>>(stream, JsonOptions.Default)
                   ?? throw new InvalidOperationException($"Failed to deserialize {path}");

        _cache = list.AsReadOnly();
        return _cache;
    }
}

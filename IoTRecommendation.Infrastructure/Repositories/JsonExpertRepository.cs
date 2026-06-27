using System.Text.Json;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IoTRecommendation.Infrastructure.Repositories;

public sealed class JsonExpertRepository : IExpertRepository
{
    private readonly DataPathOptions _paths;
    private IReadOnlyList<Expert>? _cache;

    public JsonExpertRepository(IOptions<DataPathOptions> options) => _paths = options.Value;

    public async Task<IReadOnlyList<Expert>> GetAllAsync()
    {
        if (_cache is not null) return _cache;

        string expertsDir = Path.Combine(_paths.DataDirectory, _paths.ExpertsDirectory);
        if (!Directory.Exists(expertsDir))
            throw new DirectoryNotFoundException($"Experts directory not found: {expertsDir}");

        var files = Directory.GetFiles(expertsDir, "*.json").OrderBy(f => f).ToList();
        if (files.Count == 0)
            throw new InvalidOperationException($"No expert JSON files found in {expertsDir}");

        var experts = new List<Expert>();
        foreach (var file in files)
        {
            await using var stream = File.OpenRead(file);
            var expert = await JsonSerializer.DeserializeAsync<Expert>(stream, JsonOptions.Default)
                         ?? throw new InvalidOperationException($"Failed to deserialize {file}");
            experts.Add(expert);
        }

        _cache = experts.AsReadOnly();
        return _cache;
    }
}

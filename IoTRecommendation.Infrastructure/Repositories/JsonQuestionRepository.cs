using System.Text.Json;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models.Questionnaire;
using IoTRecommendation.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IoTRecommendation.Infrastructure.Repositories;

public sealed class JsonQuestionRepository : IQuestionRepository
{
    private readonly DataPathOptions _paths;
    private IReadOnlyList<Question>? _cache;

    public JsonQuestionRepository(IOptions<DataPathOptions> options) => _paths = options.Value;

    public async Task<IReadOnlyList<Question>> GetAllAsync()
    {
        if (_cache is not null) return _cache;

        string filePath = Path.Combine(_paths.DataDirectory, _paths.QuestionsFile);
        await using var stream = File.OpenRead(filePath);
        var list = await JsonSerializer.DeserializeAsync<List<Question>>(stream, JsonOptions.Default)
                   ?? throw new InvalidOperationException($"Failed to deserialize {filePath}");

        _cache = list.OrderBy(q => q.Order).ToList().AsReadOnly();
        return _cache;
    }
}

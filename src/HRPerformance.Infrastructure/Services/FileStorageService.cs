using HRPerformance.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HRPerformance.Infrastructure.Services;
public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    public FileStorageService(IConfiguration config) => _basePath = config["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    public async Task<string> SaveAsync(Stream file, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var path = Path.Combine(dir, uniqueName);
        await using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs, ct);
        return Path.Combine(folder, uniqueName).Replace('\\', '/');
    }
    public Task<(Stream Stream, string ContentType, string FileName)?> GetAsync(string filePath, CancellationToken ct = default)
    {
        var full = Path.Combine(_basePath, filePath);
        if (!File.Exists(full)) return Task.FromResult<(Stream, string, string)?>(null);
        return Task.FromResult<(Stream, string, string)?>((new FileStream(full, FileMode.Open, FileAccess.Read), "application/octet-stream", Path.GetFileName(full)));
    }
    public Task DeleteAsync(string filePath, CancellationToken ct = default)
    { var full = Path.Combine(_basePath, filePath); if (File.Exists(full)) File.Delete(full); return Task.CompletedTask; }
}

namespace HRPerformance.Application.Interfaces;
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream file, string fileName, string contentType, string folder, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)?> GetAsync(string filePath, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
}

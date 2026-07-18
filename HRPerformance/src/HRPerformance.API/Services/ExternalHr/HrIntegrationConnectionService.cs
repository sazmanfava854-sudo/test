using HRPerformance.DTOs.HrIntegration;
using Microsoft.Extensions.Configuration;

namespace HRPerformance.Services.ExternalHr;

public class HrIntegrationConnectionService
{
    private readonly IConfiguration _configuration;

    public HrIntegrationConnectionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsEnabled() => _configuration.GetValue<bool>("HrIntegration:Enabled");

    public MisConnectionStatusDto GetStatus()
    {
        var connectionString = BuildConnectionString();
        return new MisConnectionStatusDto(
            IsEnabled() && !string.IsNullOrWhiteSpace(connectionString),
            _configuration["HrIntegration:SourceType"] ?? "SQLView");
    }

    public HrIntegrationRuntimeSettings BuildForSync(MisSyncDateRangeRequest request)
    {
        var connectionString = BuildConnectionString();
        return new HrIntegrationRuntimeSettings
        {
            IsConnectionConfigured = IsEnabled() && !string.IsNullOrWhiteSpace(connectionString),
            SourceType = _configuration["HrIntegration:SourceType"] ?? "SQLView",
            ShamsiYearPrefix = string.IsNullOrWhiteSpace(request.ShamsiYearPrefix) ? "1404" : request.ShamsiYearPrefix.Trim(),
            ProvinceCode = string.IsNullOrWhiteSpace(request.ProvinceCode) ? "147" : request.ProvinceCode.Trim(),
            ApplyProvinceFilter = request.ApplyProvinceFilter,
            ApplyShamsiYearFilter = request.ApplyShamsiYearFilter,
            EmployeeLimit = Math.Max(0, request.EmployeeLimit),
            MisConnectionString = connectionString
        };
    }

    private string? BuildConnectionString()
    {
        var section = _configuration.GetSection("HrIntegration");
        var connectionString = section["ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        var server = section["Server"];
        var database = section["Database"] ?? "MIS";
        var userId = section["UserId"];
        var password = section["Password"];
        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            return null;

        return $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
    }
}

public record MisConnectionStatusDto(bool IsConnectionConfigured, string SourceType);

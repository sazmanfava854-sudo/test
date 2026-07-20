using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public class HrIntegrationConnectionService
{
    private static readonly string[] PlaceholderPasswords = ["CHANGE_ME", "changeme", "your_password", ""];

    private readonly IConfiguration _configuration;

    public HrIntegrationConnectionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsEnabled() => _configuration.GetValue<bool>("HrIntegration:Enabled");

    public MisConnectionStatusDto GetStatus()
    {
        var section = _configuration.GetSection("HrIntegration");
        var missingFields = new List<string>();
        var enabled = IsEnabled();
        if (!enabled)
            missingFields.Add("Enabled=false");

        var connectionString = section["ConnectionString"];
        var server = section["Server"];
        var userId = section["UserId"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (string.IsNullOrWhiteSpace(server))
                missingFields.Add("Server");
            if (string.IsNullOrWhiteSpace(userId))
                missingFields.Add("UserId");
            if (string.IsNullOrWhiteSpace(password))
                missingFields.Add("Password");
            else if (IsPlaceholderPassword(password))
                missingFields.Add("Password (هنوز CHANGE_ME است)");
        }

        var builtConnectionString = BuildConnectionString();
        var isConfigured = enabled && !string.IsNullOrWhiteSpace(builtConnectionString);

        return new MisConnectionStatusDto(
            isConfigured,
            section["SourceType"] ?? "SQLView",
            missingFields,
            server,
            section["Database"] ?? "MIS",
            userId,
            IsPlaceholderPassword(password));
    }

    public HrIntegrationRuntimeSettings BuildForSync(MisSyncDateRangeRequest request)
    {
        var connectionString = BuildConnectionString();
        var status = GetStatus();
        return new HrIntegrationRuntimeSettings
        {
            IsConnectionConfigured = status.IsConnectionConfigured,
            SourceType = status.SourceType,
            ProvinceCode = MisSyncDefaults.PersonnelGroupCode,
            ApplyProvinceFilter = true,
            ApplyShamsiYearFilter = false,
            EmployeeLimit = Math.Max(0, request.EmployeeLimit),
            MisConnectionString = connectionString
        };
    }

    public HrIntegrationRuntimeSettings BuildForRosterSync(AttendanceIntegrationSetting? integrationSettings)
    {
        var connectionString = BuildConnectionString();
        var status = GetStatus();
        var provinceCode = string.IsNullOrWhiteSpace(integrationSettings?.ProvinceCode)
            ? MisSyncDefaults.PersonnelGroupCode
            : integrationSettings.ProvinceCode.Trim();

        return new HrIntegrationRuntimeSettings
        {
            IsConnectionConfigured = status.IsConnectionConfigured,
            SourceType = status.SourceType,
            ProvinceCode = provinceCode,
            ApplyProvinceFilter = integrationSettings?.ApplyProvinceFilter ?? true,
            ApplyShamsiYearFilter = false,
            EmployeeLimit = 0,
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
        if (IsPlaceholderPassword(password))
            return null;

        return $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true;Connection Timeout=30";
    }

    private static bool IsPlaceholderPassword(string? password) =>
        string.IsNullOrWhiteSpace(password) ||
        PlaceholderPasswords.Contains(password.Trim(), StringComparer.OrdinalIgnoreCase);
}

public record MisConnectionStatusDto(
    bool IsConnectionConfigured,
    string SourceType,
    IReadOnlyList<string> MissingFields,
    string? Server,
    string Database,
    string? UserId,
    bool PasswordIsPlaceholder);

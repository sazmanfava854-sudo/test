using HRPerformance.Data;
using HRPerformance.DTOs.HrIntegration;
using HRPerformance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HRPerformance.Services.ExternalHr;

public class HrIntegrationSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public HrIntegrationSettingsService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public bool IsGloballyEnabled() => _configuration.GetValue<bool>("HrIntegration:Enabled");

    public async Task<HrIntegrationSettingsDto> GetDtoAsync(Guid organizationId, CancellationToken ct = default)
    {
        var entity = await GetOrCreateEntityAsync(organizationId, ct);
        var runtime = MapRuntime(entity);
        return new HrIntegrationSettingsDto(
            entity.Id,
            entity.SourceType,
            entity.SyncMode,
            entity.ShamsiYearPrefix,
            entity.ProvinceCode,
            entity.ApplyProvinceFilter,
            entity.ApplyShamsiYearFilter,
            entity.InitialSyncMonthsBack,
            entity.MonthsPerSyncRun,
            entity.SyncDaysBack,
            entity.EmployeeLimit,
            entity.BackgroundSyncEnabled,
            entity.SyncIntervalMinutes,
            runtime.IsConnectionConfigured,
            entity.LastSyncAt);
    }

    public async Task<HrIntegrationSettingsDto> UpdateAsync(
        Guid organizationId,
        UpdateHrIntegrationSettingsRequest request,
        CancellationToken ct = default)
    {
        var entity = await GetOrCreateEntityAsync(organizationId, ct);
        entity.SyncMode = NormalizeSyncMode(request.SyncMode);
        entity.ShamsiYearPrefix = string.IsNullOrWhiteSpace(request.ShamsiYearPrefix) ? "1404" : request.ShamsiYearPrefix.Trim();
        entity.ProvinceCode = string.IsNullOrWhiteSpace(request.ProvinceCode) ? "147" : request.ProvinceCode.Trim();
        entity.ApplyProvinceFilter = request.ApplyProvinceFilter;
        entity.ApplyShamsiYearFilter = request.ApplyShamsiYearFilter;
        entity.InitialSyncMonthsBack = Math.Max(1, request.InitialSyncMonthsBack);
        entity.MonthsPerSyncRun = Math.Max(1, request.MonthsPerSyncRun);
        entity.SyncDaysBack = Math.Max(1, request.SyncDaysBack);
        entity.EmployeeLimit = Math.Max(0, request.EmployeeLimit);
        entity.BackgroundSyncEnabled = request.BackgroundSyncEnabled;
        entity.SyncIntervalMinutes = Math.Max(1, request.SyncIntervalMinutes);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return await GetDtoAsync(organizationId, ct);
    }

    public async Task<HrIntegrationRuntimeSettings> GetRuntimeSettingsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var entity = await GetOrCreateEntityAsync(organizationId, ct);
        return MapRuntime(entity);
    }

    private async Task<AttendanceIntegrationSetting> GetOrCreateEntityAsync(Guid organizationId, CancellationToken ct)
    {
        var entity = await _context.AttendanceIntegrationSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, ct);

        if (entity != null)
            return entity;

        entity = new AttendanceIntegrationSetting
        {
            OrganizationId = organizationId,
            SourceType = _configuration["HrIntegration:SourceType"] ?? "SQLView",
            SqlViewName = _configuration["HrIntegration:ViewName"] ?? "MIS.dbo.HZG_View_HourlyLeave",
            SyncMode = "Monthly",
            ShamsiYearPrefix = "1404",
            ProvinceCode = "147",
            ApplyProvinceFilter = true,
            ApplyShamsiYearFilter = true,
            InitialSyncMonthsBack = 12,
            MonthsPerSyncRun = 1,
            SyncDaysBack = 30,
            EmployeeLimit = 0,
            BackgroundSyncEnabled = false,
            SyncIntervalMinutes = 5,
            IsActive = true
        };

        _context.AttendanceIntegrationSettings.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    private HrIntegrationRuntimeSettings MapRuntime(AttendanceIntegrationSetting entity)
    {
        var section = _configuration.GetSection("HrIntegration");
        var connectionString = section["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var server = section["Server"];
            var database = section["Database"] ?? "MIS";
            var userId = section["UserId"];
            var password = section["Password"];
            if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(password))
            {
                connectionString =
                    $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
            }
        }

        return new HrIntegrationRuntimeSettings
        {
            IsConnectionConfigured = IsGloballyEnabled() && !string.IsNullOrWhiteSpace(connectionString),
            SourceType = entity.SourceType,
            SyncMode = entity.SyncMode,
            ShamsiYearPrefix = entity.ShamsiYearPrefix,
            ProvinceCode = entity.ProvinceCode,
            ApplyProvinceFilter = entity.ApplyProvinceFilter,
            ApplyShamsiYearFilter = entity.ApplyShamsiYearFilter,
            InitialSyncMonthsBack = entity.InitialSyncMonthsBack,
            MonthsPerSyncRun = entity.MonthsPerSyncRun,
            SyncDaysBack = entity.SyncDaysBack,
            EmployeeLimit = entity.EmployeeLimit,
            BackgroundSyncEnabled = entity.BackgroundSyncEnabled,
            SyncIntervalMinutes = entity.SyncIntervalMinutes,
            SyncStartupDelaySeconds = 15,
            MisConnectionString = connectionString
        };
    }

    private static string NormalizeSyncMode(string? syncMode) =>
        syncMode?.Trim().ToUpperInvariant() switch
        {
            "DAYSBACK" => "DaysBack",
            "DATERANGE" => "DateRange",
            _ => "Monthly"
        };
}

using System.Net.Http.Json;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;
public class AttendanceSyncService : IAttendanceSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IRuleEngineService _ruleEngine;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AttendanceSyncService> _logger;

    public AttendanceSyncService(ApplicationDbContext context, IRuleEngineService ruleEngine, IHttpClientFactory httpClientFactory, ILogger<AttendanceSyncService> logger)
    { _context = context; _ruleEngine = ruleEngine; _httpClientFactory = httpClientFactory; _logger = logger; }

    public async Task SyncAsync(Guid organizationId, CancellationToken ct = default)
    {
        var settings = await _context.AttendanceIntegrationSettings.FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.IsActive, ct);
        if (settings == null) return;
        var syncLog = new AttendanceSyncLog { OrganizationId = organizationId, SyncStartedAt = DateTime.UtcNow, SourceType = settings.SourceType };
        try
        {
            if (settings.SourceType == "REST" && !string.IsNullOrEmpty(settings.EndpointUrl))
                await SyncFromRestAsync(settings, syncLog, ct);
            else if (settings.SourceType == "SQLView")
                await SyncFromSqlViewAsync(settings, syncLog, ct);
            syncLog.Status = syncLog.RecordsFailed > 0 ? AttendanceSyncStatus.Partial : AttendanceSyncStatus.Success;
            settings.LastSyncAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            syncLog.Status = AttendanceSyncStatus.Failed;
            syncLog.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Attendance sync failed for organization {OrgId}", organizationId);
        }
        syncLog.SyncCompletedAt = DateTime.UtcNow;
        _context.AttendanceSyncLogs.Add(syncLog);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SyncFromRestAsync(AttendanceIntegrationSetting settings, AttendanceSyncLog log, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("AttendanceSync");
        if (!string.IsNullOrEmpty(settings.ApiKey)) client.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
        var records = await client.GetFromJsonAsync<List<AttendanceRecordDto>>(settings.EndpointUrl!, ct) ?? new();
        foreach (var record in records) await ProcessRecordAsync(settings.OrganizationId, record, log, ct);
    }

    private async Task SyncFromSqlViewAsync(AttendanceIntegrationSetting settings, AttendanceSyncLog log, CancellationToken ct)
    {
        _logger.LogInformation("SQL View sync configured for {View}", settings.SqlViewName);
        await Task.CompletedTask;
    }

    private async Task ProcessRecordAsync(Guid orgId, AttendanceRecordDto record, AttendanceSyncLog log, CancellationToken ct)
    {
        try
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.PersonnelCode == record.PersonnelCode && e.OrganizationId == orgId && !e.IsDeleted, ct);
            if (emp == null) { log.RecordsFailed++; return; }
            var existing = await _context.AttendanceLogs.FirstOrDefaultAsync(a => a.EmployeeId == emp.Id && a.AttendanceDate == record.Date, ct);
            if (existing != null) { existing.EntryTime = record.EntryTime; existing.ExitTime = record.ExitTime; existing.DelayMinutes = record.DelayMinutes; existing.IsAbsent = record.IsAbsent; existing.WorkingHours = record.WorkingHours; }
            else _context.AttendanceLogs.Add(new AttendanceLog { EmployeeId = emp.Id, OrganizationId = orgId, AttendanceDate = record.Date, EntryTime = record.EntryTime, ExitTime = record.ExitTime, WorkingHours = record.WorkingHours, OvertimeHours = record.OvertimeHours, DelayMinutes = record.DelayMinutes, IsAbsent = record.IsAbsent, IsOnMission = record.IsOnMission, IsOnLeave = record.IsOnLeave, ExternalId = record.ExternalId });
            await _context.SaveChangesAsync(ct);
            var attLog = await _context.AttendanceLogs.FirstAsync(a => a.EmployeeId == emp.Id && a.AttendanceDate == record.Date, ct);
            await _ruleEngine.ProcessAttendanceAsync(attLog, ct);
            log.RecordsProcessed++;
        }
        catch { log.RecordsFailed++; }
    }

    private record AttendanceRecordDto(string PersonnelCode, DateTime Date, TimeSpan? EntryTime, TimeSpan? ExitTime, decimal? WorkingHours, decimal? OvertimeHours, int DelayMinutes, bool IsAbsent, bool IsOnMission, bool IsOnLeave, string? ExternalId);
}

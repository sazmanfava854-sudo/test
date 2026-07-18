using System.Net.Http.Json;
using HRPerformance.Entities;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using HRPerformance.Data;
using HRPerformance.Services.ExternalHr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Services;

public class AttendanceSyncService : IAttendanceSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IRuleEngineService _ruleEngine;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MisHrDataReader _misHrDataReader;
    private readonly MisHrEmployeeSyncService _employeeSync;
    private readonly MisSyncStateService _syncStateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AttendanceSyncService> _logger;

    public AttendanceSyncService(
        ApplicationDbContext context,
        IRuleEngineService ruleEngine,
        IHttpClientFactory httpClientFactory,
        MisHrDataReader misHrDataReader,
        MisHrEmployeeSyncService employeeSync,
        MisSyncStateService syncStateService,
        IConfiguration configuration,
        ILogger<AttendanceSyncService> logger)
    {
        _context = context;
        _ruleEngine = ruleEngine;
        _httpClientFactory = httpClientFactory;
        _misHrDataReader = misHrDataReader;
        _employeeSync = employeeSync;
        _syncStateService = syncStateService;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<AttendanceSyncResult> SyncAsync(Guid organizationId, CancellationToken ct = default)
        => SyncInternalAsync(organizationId, ranges: null, ct);

    public Task<AttendanceSyncResult> SyncMonthAsync(Guid organizationId, int shamsiYear, int shamsiMonth, CancellationToken ct = default)
    {
        var (start, endExclusive) = ShamsiDateHelper.GetGregorianMonthRange(shamsiYear, shamsiMonth);
        var range = new MisSyncRange
        {
            SyncFrom = start,
            SyncToExclusive = endExclusive,
            ShamsiYear = shamsiYear,
            ShamsiMonth = shamsiMonth,
            Description = $"ماه {shamsiYear}/{shamsiMonth:00}"
        };
        return SyncInternalAsync(organizationId, [range], ct, updateState: false);
    }

    private async Task<AttendanceSyncResult> SyncInternalAsync(
        Guid organizationId,
        IReadOnlyList<MisSyncRange>? ranges,
        CancellationToken ct,
        bool updateState = true)
    {
        var result = new AttendanceSyncResult();
        var settings = await _context.AttendanceIntegrationSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.IsActive, ct);

        var sourceType = settings?.SourceType ?? _configuration["HrIntegration:SourceType"] ?? "SQLView";
        if (settings == null && !IsHrIntegrationEnabled())
            return result;

        var syncLog = new AttendanceSyncLog
        {
            OrganizationId = organizationId,
            SyncStartedAt = DateTime.UtcNow,
            SourceType = sourceType
        };

        try
        {
            switch (sourceType.ToUpperInvariant())
            {
                case "REST":
                    if (settings != null && !string.IsNullOrEmpty(settings.EndpointUrl))
                        await SyncFromRestAsync(settings, syncLog, ct);
                    break;
                case "SQLVIEW":
                    await SyncFromMisSqlViewAsync(organizationId, syncLog, result, ranges, updateState, ct);
                    break;
                default:
                    _logger.LogWarning("Unsupported attendance source type: {SourceType}", sourceType);
                    break;
            }

            syncLog.Status = syncLog.RecordsFailed > 0 ? AttendanceSyncStatus.Partial : AttendanceSyncStatus.Success;
            if (settings != null) settings.LastSyncAt = DateTime.UtcNow;
            result.RecordsProcessed = syncLog.RecordsProcessed;
            result.RecordsFailed = syncLog.RecordsFailed;
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
        return result;
    }

    private bool IsHrIntegrationEnabled() =>
        _configuration.GetValue<bool>("HrIntegration:Enabled");

    private async Task SyncFromMisSqlViewAsync(
        Guid organizationId,
        AttendanceSyncLog log,
        AttendanceSyncResult result,
        IReadOnlyList<MisSyncRange>? explicitRanges,
        bool updateState,
        CancellationToken ct)
    {
        var ranges = explicitRanges ?? await _syncStateService.GetNextRangesAsync(organizationId, ct);
        if (ranges.Count == 0)
        {
            _logger.LogInformation("No MIS sync ranges scheduled for organization {OrgId}", organizationId);
            return;
        }

        var syncedRanges = new List<string>();
        foreach (var range in ranges)
        {
            _logger.LogInformation("MIS SQL sync {Range} for organization {OrgId}", range.Description, organizationId);

            var records = await _misHrDataReader.ReadHourlyLeavesAsync(range, ct);
            if (records.Count == 0)
            {
                _logger.LogWarning(
                    "MIS returned 0 records for {Range} (org {OrgId}). Check filters or run diagnostic.",
                    range.Description, organizationId);
            }

            foreach (var record in records)
                await ProcessMisHourlyLeaveAsync(organizationId, record, log, ct);

            syncedRanges.Add($"{range.Description} ({records.Count} رکورد)");
            if (updateState)
                await _syncStateService.MarkRangeCompletedAsync(organizationId, range, ct);
        }

        result.SyncedRanges = syncedRanges;
        var state = await _syncStateService.GetOrCreateStateAsync(organizationId, ct);
        result.IsBackfillComplete = state.IsBackfillComplete;
        if (!state.IsBackfillComplete)
            result.NextRangeDescription = $"ماه {state.TargetShamsiYear}/{state.NextShamsiMonth:00}";

        _logger.LogInformation(
            "MIS sync finished for {OrgId}: ranges={Ranges}, processed={Processed}, failed={Failed}, backfillComplete={BackfillComplete}",
            organizationId, syncedRanges.Count, log.RecordsProcessed, log.RecordsFailed, result.IsBackfillComplete);
    }

    private async Task ProcessMisHourlyLeaveAsync(Guid organizationId, MisHourlyLeaveRecord record, AttendanceSyncLog log, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(record.PerCod))
            {
                log.RecordsFailed++;
                return;
            }

            var employee = await _employeeSync.UpsertEmployeeAsync(organizationId, record, ct);
            var externalId = record.Id.ToString("0");

            var attendance = await _context.AttendanceLogs
                .FirstOrDefaultAsync(a => a.OrganizationId == organizationId && a.ExternalId == externalId, ct);

            var entryTime = ParseTime(record.StartTime);
            var exitTime = ParseTime(record.EndTime);
            var workingHours = entryTime.HasValue && exitTime.HasValue
                ? (decimal?)(exitTime.Value - entryTime.Value).TotalHours
                : null;

            if (attendance == null)
            {
                attendance = new AttendanceLog
                {
                    EmployeeId = employee.Id,
                    OrganizationId = organizationId,
                    AttendanceDate = record.StartDate.Date,
                    EntryTime = entryTime,
                    ExitTime = exitTime,
                    WorkingHours = workingHours,
                    DelayMinutes = 0,
                    IsAbsent = false,
                    IsOnLeave = true,
                    IsOnMission = false,
                    LeaveType = $"مرخصی ساعتی - نوع {record.FirstTimeType}",
                    Source = "MIS-SQLView",
                    ExternalId = externalId
                };
                _context.AttendanceLogs.Add(attendance);
            }
            else
            {
                attendance.EntryTime = entryTime;
                attendance.ExitTime = exitTime;
                attendance.WorkingHours = workingHours;
                attendance.IsOnLeave = true;
                attendance.LeaveType = $"مرخصی ساعتی - نوع {record.FirstTimeType}";
                attendance.UpdatedAt = DateTime.UtcNow;
                attendance.IsProcessed = false;
            }

            await _context.SaveChangesAsync(ct);
            await _ruleEngine.ProcessHourlyLeaveAsync(attendance, record.LeaveDurationMinutes, ct);
            log.RecordsProcessed++;
        }
        catch (Exception ex)
        {
            log.RecordsFailed++;
            _logger.LogError(ex, "Failed to process MIS record {Id} for PerCod {PerCod}", record.Id, record.PerCod);
        }
    }

    private async Task SyncFromRestAsync(AttendanceIntegrationSetting settings, AttendanceSyncLog log, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("AttendanceSync");
        if (!string.IsNullOrEmpty(settings.ApiKey))
            client.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);

        var records = await client.GetFromJsonAsync<List<AttendanceRecordDto>>(settings.EndpointUrl!, ct) ?? new();
        foreach (var record in records)
            await ProcessRestRecordAsync(settings.OrganizationId, record, log, ct);
    }

    private async Task ProcessRestRecordAsync(Guid orgId, AttendanceRecordDto record, AttendanceSyncLog log, CancellationToken ct)
    {
        try
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(
                e => e.PersonnelCode == record.PersonnelCode && e.OrganizationId == orgId && !e.IsDeleted, ct);
            if (emp == null) { log.RecordsFailed++; return; }

            var existing = await _context.AttendanceLogs.FirstOrDefaultAsync(
                a => a.EmployeeId == emp.Id && a.AttendanceDate == record.Date, ct);

            if (existing != null)
            {
                existing.EntryTime = record.EntryTime;
                existing.ExitTime = record.EndTime;
                existing.DelayMinutes = record.DelayMinutes;
                existing.IsAbsent = record.IsAbsent;
                existing.WorkingHours = record.WorkingHours;
                existing.IsProcessed = false;
            }
            else
            {
                _context.AttendanceLogs.Add(new AttendanceLog
                {
                    EmployeeId = emp.Id,
                    OrganizationId = orgId,
                    AttendanceDate = record.Date,
                    EntryTime = record.EntryTime,
                    ExitTime = record.EndTime,
                    WorkingHours = record.WorkingHours,
                    OvertimeHours = record.OvertimeHours,
                    DelayMinutes = record.DelayMinutes,
                    IsAbsent = record.IsAbsent,
                    IsOnMission = record.IsOnMission,
                    IsOnLeave = record.IsOnLeave,
                    ExternalId = record.ExternalId,
                    Source = "REST"
                });
            }

            await _context.SaveChangesAsync(ct);
            var attLog = await _context.AttendanceLogs.FirstAsync(
                a => a.EmployeeId == emp.Id && a.AttendanceDate == record.Date, ct);
            await _ruleEngine.ProcessAttendanceAsync(attLog, ct);
            log.RecordsProcessed++;
        }
        catch
        {
            log.RecordsFailed++;
        }
    }

    private static TimeSpan? ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return TimeSpan.TryParse(value, out var time) ? time : null;
    }

    private record AttendanceRecordDto(
        string PersonnelCode,
        DateTime Date,
        TimeSpan? EntryTime,
        TimeSpan? EndTime,
        decimal? WorkingHours,
        decimal? OvertimeHours,
        int DelayMinutes,
        bool IsAbsent,
        bool IsOnMission,
        bool IsOnLeave,
        string? ExternalId);
}

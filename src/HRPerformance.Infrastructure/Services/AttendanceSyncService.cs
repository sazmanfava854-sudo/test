using System.Net.Http.Json;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Domain.Models;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Services.ExternalHr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;

public class AttendanceSyncService : IAttendanceSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IRuleEngineService _ruleEngine;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MisHrDataReader _misHrDataReader;
    private readonly MisHrEmployeeSyncService _employeeSync;
    private readonly HrIntegrationConnectionService _connectionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AttendanceSyncService> _logger;

    public AttendanceSyncService(
        ApplicationDbContext context,
        IRuleEngineService ruleEngine,
        IHttpClientFactory httpClientFactory,
        MisHrDataReader misHrDataReader,
        MisHrEmployeeSyncService employeeSync,
        HrIntegrationConnectionService connectionService,
        IConfiguration configuration,
        ILogger<AttendanceSyncService> logger)
    {
        _context = context;
        _ruleEngine = ruleEngine;
        _httpClientFactory = httpClientFactory;
        _misHrDataReader = misHrDataReader;
        _employeeSync = employeeSync;
        _connectionService = connectionService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AttendanceSyncResult> SyncDateRangeAsync(
        Guid organizationId,
        MisSyncDateRangeRequest request,
        CancellationToken ct = default)
    {
        var result = new AttendanceSyncResult();
        if (request.ToDate < request.FromDate)
            throw new ArgumentException("تاریخ پایان باید بعد از تاریخ شروع باشد");

        var runtimeSettings = _connectionService.BuildForSync(request);
        if (!runtimeSettings.IsConnectionConfigured)
            throw new InvalidOperationException("اتصال MIS پیکربندی نشده است");

        var entity = await _context.AttendanceIntegrationSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, ct);
        var sourceType = entity?.SourceType ?? runtimeSettings.SourceType;

        var range = new MisSyncRange
        {
            SyncFrom = request.FromDate.Date,
            SyncToExclusive = request.ToDate.Date.AddDays(1),
            Description = $"بازه {request.FromDate:yyyy-MM-dd} تا {request.ToDate:yyyy-MM-dd}"
        };

        var syncLog = new AttendanceSyncLog
        {
            OrganizationId = organizationId,
            SyncStartedAt = DateTime.UtcNow,
            SourceType = sourceType
        };

        try
        {
            if (sourceType.Equals("SQLVIEW", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("MIS sync {Range} for organization {OrgId}", range.Description, organizationId);
                var records = await _misHrDataReader.ReadHourlyLeavesAsync(runtimeSettings, range, ct);
                foreach (var record in records)
                    await ProcessMisHourlyLeaveAsync(organizationId, record, syncLog, ct);

                result.SyncedRanges = [$"{range.Description} ({records.Count} رکورد)"];
                result.RecordsProcessed = syncLog.RecordsProcessed;
                result.RecordsFailed = syncLog.RecordsFailed;
            }
            else if (sourceType.Equals("REST", StringComparison.OrdinalIgnoreCase) && entity != null)
            {
                await SyncFromRestAsync(entity, syncLog, ct);
                result.RecordsProcessed = syncLog.RecordsProcessed;
                result.RecordsFailed = syncLog.RecordsFailed;
            }

            syncLog.Status = syncLog.RecordsFailed > 0 ? AttendanceSyncStatus.Partial : AttendanceSyncStatus.Success;
            if (entity != null) entity.LastSyncAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            syncLog.Status = AttendanceSyncStatus.Failed;
            syncLog.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Attendance sync failed for organization {OrgId}", organizationId);
            throw;
        }
        finally
        {
            syncLog.SyncCompletedAt = DateTime.UtcNow;
            _context.AttendanceSyncLogs.Add(syncLog);
            await _context.SaveChangesAsync(ct);
        }

        return result;
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

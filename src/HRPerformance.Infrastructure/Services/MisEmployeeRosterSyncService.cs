using System.Diagnostics;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Services.ExternalHr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;

public class EmployeeRosterSyncResult
{
    public bool Success { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Total { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string ProvinceCode { get; set; } = "147";
    public DateTime? LastSyncAt { get; set; }
}

public class MisEmployeeRosterSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly MisHrDataReader _misReader;
    private readonly HrIntegrationConnectionService _connectionService;
    private readonly ILogger<MisEmployeeRosterSyncService> _logger;

    public MisEmployeeRosterSyncService(
        ApplicationDbContext context,
        MisHrDataReader misReader,
        HrIntegrationConnectionService connectionService,
        ILogger<MisEmployeeRosterSyncService> logger)
    {
        _context = context;
        _misReader = misReader;
        _connectionService = connectionService;
        _logger = logger;
    }

    public async Task<EmployeeRosterSyncResult> SyncFromMisAsync(
        Guid organizationId,
        Guid? requestedByUserId,
        string? requestedByUserName,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new EmployeeRosterSyncResult();

        if (organizationId == Guid.Empty)
        {
            result.ErrorMessage = "شناسه سازمان یافت نشد";
            return result;
        }

        var settings = await _context.AttendanceIntegrationSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, ct);

        if (settings == null)
        {
            settings = new AttendanceIntegrationSetting
            {
                OrganizationId = organizationId,
                SourceType = "SQLView",
                SqlViewName = "MIS.dbo.HZG_View_HourlyLeave",
                ProvinceCode = MisSyncDefaults.PersonnelGroupCode,
                ApplyProvinceFilter = true,
                IsActive = true
            };
            _context.AttendanceIntegrationSettings.Add(settings);
            await _context.SaveChangesAsync(ct);
        }

        if (settings.IsRosterSyncRunning)
        {
            result.ErrorMessage = "هم‌اکنون دریافت فهرست پرسنل در حال اجراست — چند لحظه بعد دوباره تلاش کنید";
            return result;
        }

        settings.IsRosterSyncRunning = true;
        await _context.SaveChangesAsync(ct);

        var syncLog = new AttendanceSyncLog
        {
            OrganizationId = organizationId,
            SyncStartedAt = DateTime.UtcNow,
            Status = AttendanceSyncStatus.Partial,
            SourceType = settings.SourceType,
            SyncType = "EmployeeRoster",
            RequestedByUserName = requestedByUserName
        };
        _context.AttendanceSyncLogs.Add(syncLog);

        try
        {
            var runtime = _connectionService.BuildForRosterSync(settings);
            result.ProvinceCode = runtime.ProvinceCode;

            if (!runtime.IsConnectionConfigured)
                throw new InvalidOperationException("اتصال MIS پیکربندی نشده است");

            await EvaluationCategoryBootstrap.EnsureForOrganizationAsync(_context, organizationId, ct);

            var misRecords = await _misReader.ReadRosterEmployeesAsync(runtime, ct);
            result.Total = misRecords.Count;

            var personnelCodes = misRecords
                .Select(r => r.PerCod.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existing = await _context.Employees
                .Where(e => e.OrganizationId == organizationId && personnelCodes.Contains(e.PersonnelCode))
                .ToListAsync(ct);

            var existingByCode = existing.ToDictionary(e => e.PersonnelCode, StringComparer.OrdinalIgnoreCase);
            var syncTime = DateTime.UtcNow;

            foreach (var record in misRecords)
            {
                if (string.IsNullOrWhiteSpace(record.PerCod))
                    continue;

                var personnelCode = record.PerCod.Trim();
                if (!existingByCode.TryGetValue(personnelCode, out var employee))
                {
                    employee = CreateFromMis(organizationId, record, syncTime);
                    _context.Employees.Add(employee);
                    existingByCode[personnelCode] = employee;
                    result.Inserted++;
                }
                else if (ApplyMisFields(employee, record, syncTime))
                {
                    result.Updated++;
                }
            }

            settings.LastEmployeeRosterSyncAt = syncTime;
            result.LastSyncAt = syncTime;

            syncLog.Status = AttendanceSyncStatus.Success;
            syncLog.EmployeesInserted = result.Inserted;
            syncLog.EmployeesUpdated = result.Updated;
            syncLog.RecordsProcessed = result.Total;
            syncLog.SyncCompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            result.Success = true;
            _logger.LogInformation(
                "Roster sync org {OrgId}: {Inserted} inserted, {Updated} updated, {Total} from MIS (ProvinceCode={ProvinceCode})",
                organizationId, result.Inserted, result.Updated, result.Total, result.ProvinceCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Roster sync failed for org {OrgId}", organizationId);
            syncLog.Status = AttendanceSyncStatus.Failed;
            syncLog.ErrorMessage = ex.GetBaseException().Message;
            syncLog.SyncCompletedAt = DateTime.UtcNow;
            result.ErrorMessage = ex.GetBaseException().Message;
            await _context.SaveChangesAsync(ct);
        }
        finally
        {
            settings.IsRosterSyncRunning = false;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    private static Employee CreateFromMis(Guid organizationId, MisHourlyLeaveRecord record, DateTime syncTime) =>
        new()
        {
            OrganizationId = organizationId,
            PersonnelCode = record.PerCod.Trim(),
            NationalCode = NormalizeNationalCode(record.NationalIDNo, record.PerCod),
            FirstName = record.Name?.Trim() ?? "نامشخص",
            LastName = record.LastName?.Trim() ?? "نامشخص",
            EmploymentDate = record.StartDate.Date,
            EmploymentType = EmploymentType.Permanent,
            Status = EmployeeStatus.Active,
            LastSeenInRosterSyncAt = syncTime,
            Description = $"سینک roster MIS - ProvinceCode: {record.ProvinceCode}"
        };

    /// <summary>
    /// MIS برنده برای هویت؛ Status لوکال اگر Inactive/Terminated باشد دست نخورده می‌ماند.
    /// </summary>
    private static bool ApplyMisFields(Employee employee, MisHourlyLeaveRecord record, DateTime syncTime)
    {
        var changed = false;

        if (!string.IsNullOrWhiteSpace(record.Name) && employee.FirstName != record.Name.Trim())
        {
            employee.FirstName = record.Name.Trim();
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(record.LastName) && employee.LastName != record.LastName.Trim())
        {
            employee.LastName = record.LastName.Trim();
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(record.NationalIDNo))
        {
            var national = record.NationalIDNo.Trim();
            if (employee.NationalCode != national)
            {
                employee.NationalCode = national;
                changed = true;
            }
        }

        if (employee.LastSeenInRosterSyncAt != syncTime)
        {
            employee.LastSeenInRosterSyncAt = syncTime;
            changed = true;
        }

        employee.UpdatedAt = DateTime.UtcNow;
        return changed;
    }

    private static string NormalizeNationalCode(string? nationalId, string personnelCode)
    {
        if (!string.IsNullOrWhiteSpace(nationalId)) return nationalId.Trim();
        return personnelCode.PadLeft(10, '0')[..10];
    }
}

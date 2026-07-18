using HRPerformance.Data;
using HRPerformance.Interfaces;
using HRPerformance.Services.ExternalHr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
public class AttendanceSyncController : ControllerBase
{
    private readonly IAttendanceSyncService _syncService;
    private readonly ICurrentUserService _currentUser;
    private readonly MisHrDataReader _misHrDataReader;
    private readonly MisSyncStateService _syncStateService;
    private readonly HrIntegrationSettingsService _hrSettingsService;
    private readonly ApplicationDbContext _context;

    public AttendanceSyncController(
        IAttendanceSyncService syncService,
        ICurrentUserService currentUser,
        MisHrDataReader misHrDataReader,
        MisSyncStateService syncStateService,
        HrIntegrationSettingsService hrSettingsService,
        ApplicationDbContext context)
    {
        _syncService = syncService;
        _currentUser = currentUser;
        _misHrDataReader = misHrDataReader;
        _syncStateService = syncStateService;
        _hrSettingsService = hrSettingsService;
        _context = context;
    }

    /// <summary>
    /// سینک دستی از MIS — در حالت Monthly فقط یک بازه (مثلاً یک ماه) را می‌گیرد
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> RunSync(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var result = await _syncService.SyncAsync(orgId, ct);
        return Ok(new
        {
            success = true,
            message = "سینک حضور و غیاب انجام شد",
            result
        });
    }

    /// <summary>
    /// سینک یک ماه شمسی مشخص
    /// </summary>
    [HttpPost("run-month")]
    public async Task<IActionResult> RunMonthSync([FromQuery] int shamsiYear, [FromQuery] int shamsiMonth, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });
        if (shamsiMonth is < 1 or > 12)
            return BadRequest(new { success = false, message = "ماه شمسی باید بین ۱ تا ۱۲ باشد" });

        var result = await _syncService.SyncMonthAsync(orgId, shamsiYear, shamsiMonth, ct);
        return Ok(new
        {
            success = true,
            message = $"سینک ماه {shamsiYear}/{shamsiMonth:00} انجام شد",
            result
        });
    }

    /// <summary>
    /// وضعیت پیشرفت سینک ماهانه
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var runtimeSettings = await _hrSettingsService.GetRuntimeSettingsAsync(orgId, ct);
        var state = await _syncStateService.GetOrCreateStateAsync(orgId, ct);
        var nextRanges = await _syncStateService.GetNextRangesAsync(orgId, ct);

        return Ok(new
        {
            success = true,
            organizationId = orgId,
            syncMode = runtimeSettings.SyncMode,
            backgroundSyncEnabled = runtimeSettings.BackgroundSyncEnabled,
            employeeLimit = runtimeSettings.EmployeeLimit,
            state = new
            {
                state.TargetShamsiYear,
                state.NextShamsiMonth,
                state.BackfillStartMonth,
                state.IsBackfillComplete,
                state.LastSyncedAt,
                state.LastSyncDescription
            },
            nextRanges = nextRanges.Select(r => new
            {
                r.Description,
                r.SyncFrom,
                r.SyncToExclusive,
                r.ShamsiYear,
                r.ShamsiMonth,
                r.IsBackfill
            }),
            employeesInDatabase = await _context.Employees.CountAsync(e => e.OrganizationId == orgId && !e.IsDeleted, ct)
        });
    }

    /// <summary>
    /// بررسی اتصال MIS و تعداد رکوردها در هر مرحله فیلتر
    /// </summary>
    [HttpGet("diagnostic")]
    public async Task<IActionResult> Diagnostic([FromQuery] int? shamsiYear, [FromQuery] int? shamsiMonth, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var runtimeSettings = await _hrSettingsService.GetRuntimeSettingsAsync(orgId, ct);
        MisSyncRange range;
        if (shamsiYear.HasValue && shamsiMonth.HasValue)
        {
            var (start, endExclusive) = ShamsiDateHelper.GetGregorianMonthRange(shamsiYear.Value, shamsiMonth.Value);
            range = new MisSyncRange
            {
                SyncFrom = start,
                SyncToExclusive = endExclusive,
                ShamsiYear = shamsiYear,
                ShamsiMonth = shamsiMonth,
                Description = $"ماه {shamsiYear}/{shamsiMonth:00}"
            };
        }
        else
        {
            var nextRanges = await _syncStateService.GetNextRangesAsync(orgId, ct);
            range = nextRanges.FirstOrDefault() ?? new MisSyncRange
            {
                SyncFrom = DateTime.Today.AddDays(-30),
                SyncToExclusive = DateTime.Today.AddDays(1),
                Description = "۳۰ روز اخیر"
            };
        }

        var diagnostic = await _misHrDataReader.GetDiagnosticAsync(runtimeSettings, range, ct);
        diagnostic.EmployeesInHrDatabase = await _context.Employees
            .CountAsync(e => e.OrganizationId == orgId && !e.IsDeleted, ct);

        return Ok(new
        {
            success = true,
            organizationId = orgId,
            diagnostic,
            hints = BuildDiagnosticHints(diagnostic)
        });
    }

    private static IReadOnlyList<string> BuildDiagnosticHints(MisHrDiagnosticResult d)
    {
        var hints = new List<string>();

        if (!d.CanConnect)
        {
            hints.Add("اتصال به دیتابیس MIS برقرار نشد. Server، UserId و Password را در appsettings بررسی کنید.");
            return hints;
        }

        if (d.TotalInView == 0)
            hints.Add("View خالی است یا کاربر MIS به آن دسترسی ندارد.");
        else if (d.CountAfterSyncFrom == 0)
            hints.Add($"هیچ رکوردی در بازه {d.SyncFrom:yyyy-MM-dd} تا {d.SyncTo:yyyy-MM-dd} نیست. ماه یا بازه دیگری را امتحان کنید.");
        else if (d.ApplyProvinceFilter && d.CountAfterProvince == 0)
            hints.Add($"ProvinceCode={d.ProvinceCode} با داده‌های MIS مطابقت ندارد. از تنظیمات > سینک MIS مقدار صحیح را وارد کنید.");
        else if (d.ApplyShamsiYearFilter && d.CountAfterShamsiYear == 0)
            hints.Add($"سال شمسی {d.ShamsiYearPrefix} در MIS یافت نشد. از تنظیمات > سینک MIS سال را اصلاح کنید.");
        else if (d.CountWithActiveFilters == 0)
            hints.Add("ترکیب فیلترهای فعال هیچ رکوردی برنمی‌گرداند. تنظیمات سینک MIS را شل‌تر کنید.");
        else if (d.EmployeesInHrDatabase == 0 && d.DistinctEmployeesWithActiveFilters > 0)
            hints.Add("MIS داده دارد ولی کارمندی در HR ثبت نشده. POST /api/attendancesync/run را اجرا کنید.");
        else if (d.EmployeesInHrDatabase > 0)
            hints.Add($"سینک قبلاً {d.EmployeesInHrDatabase} کارمند ثبت کرده است.");

        if (d.SyncMode.Equals("Monthly", StringComparison.OrdinalIgnoreCase))
            hints.Add("در حالت Monthly هر اجرا فقط یک ماه را می‌گیرد. وضعیت: GET /api/attendancesync/status");

        return hints;
    }
}

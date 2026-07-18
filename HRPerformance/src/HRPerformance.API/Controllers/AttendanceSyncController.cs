using HRPerformance.Data;
using HRPerformance.Interfaces;
using HRPerformance.Services.ExternalHr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
public class AttendanceSyncController : ControllerBase
{
    private readonly IAttendanceSyncService _syncService;
    private readonly ICurrentUserService _currentUser;
    private readonly MisHrDataReader _misHrDataReader;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AttendanceSyncController(
        IAttendanceSyncService syncService,
        ICurrentUserService currentUser,
        MisHrDataReader misHrDataReader,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _syncService = syncService;
        _currentUser = currentUser;
        _misHrDataReader = misHrDataReader;
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// سینک دستی از MIS / HR خارجی
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> RunSync(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        await _syncService.SyncAsync(orgId, ct);
        return Ok(new { success = true, message = "سینک حضور و غیاب انجام شد" });
    }

    /// <summary>
    /// بررسی اتصال MIS و تعداد رکوردها در هر مرحله فیلتر
    /// </summary>
    [HttpGet("diagnostic")]
    public async Task<IActionResult> Diagnostic(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var syncDaysBack = _configuration.GetValue<int>("HrIntegration:SyncDaysBack", 30);
        var syncFrom = DateTime.Today.AddDays(-syncDaysBack);

        var diagnostic = await _misHrDataReader.GetDiagnosticAsync(syncFrom, ct);
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
            hints.Add($"هیچ رکوردی بعد از {d.SyncFrom:yyyy-MM-dd} نیست. SyncDaysBack را افزایش دهید.");
        else if (d.ApplyProvinceFilter && d.CountAfterProvince == 0)
            hints.Add($"ProvinceCode={d.ProvinceCode} با داده‌های MIS مطابقت ندارد. مقدار صحیح را در HrIntegration:ProvinceCode تنظیم کنید یا ApplyProvinceFilter=false بگذارید.");
        else if (d.ApplyShamsiYearFilter && d.CountAfterShamsiYear == 0)
            hints.Add($"سال شمسی {d.ShamsiYearPrefix} در MIS یافت نشد. ShamsiYearPrefix را اصلاح کنید یا ApplyShamsiYearFilter=false بگذارید.");
        else if (d.CountWithActiveFilters == 0)
            hints.Add("ترکیب فیلترهای فعال هیچ رکوردی برنمی‌گرداند. فیلترها را در appsettings شل‌تر کنید.");
        else if (d.EmployeesInHrDatabase == 0 && d.DistinctEmployeesWithActiveFilters > 0)
            hints.Add("MIS داده دارد ولی کارمندی در HR ثبت نشده. POST /api/attendancesync/run را اجرا کنید.");
        else if (d.EmployeesInHrDatabase > 0)
            hints.Add($"سینک قبلاً {d.EmployeesInHrDatabase} کارمند ثبت کرده است.");

        return hints;
    }
}

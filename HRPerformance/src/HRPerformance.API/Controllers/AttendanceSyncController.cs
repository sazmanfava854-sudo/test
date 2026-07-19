using HRPerformance.Application.Features.Dashboard;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Domain.Models;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Services.ExternalHr;
using MediatR;
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
    private readonly HrIntegrationConnectionService _connectionService;
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;

    public AttendanceSyncController(
        IAttendanceSyncService syncService,
        ICurrentUserService currentUser,
        MisHrDataReader misHrDataReader,
        HrIntegrationConnectionService connectionService,
        ApplicationDbContext context,
        IMediator mediator)
    {
        _syncService = syncService;
        _currentUser = currentUser;
        _misHrDataReader = misHrDataReader;
        _connectionService = connectionService;
        _context = context;
        _mediator = mediator;
    }

    /// <summary>
    /// دریافت داده از MIS بر اساس بازه تاریخ انتخاب‌شده توسط کاربر
    /// </summary>
    [HttpPost("run-range")]
    public async Task<IActionResult> RunRangeSync([FromBody] MisSyncDateRangeRequest request, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });
        if (request.ToDate < request.FromDate)
            return BadRequest(new { success = false, message = "تاریخ پایان باید بعد از تاریخ شروع باشد" });

        try
        {
            var result = await _syncService.SyncDateRangeAsync(orgId, request, ct);
            return Ok(new
            {
                success = true,
                message = $"داده‌های بازه {request.FromDate:yyyy-MM-dd} تا {request.ToDate:yyyy-MM-dd} دریافت شد",
                result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        var connection = _connectionService.GetStatus();
        var lastSync = await _context.AttendanceIntegrationSettings
            .Where(s => s.OrganizationId == orgId)
            .Select(s => s.LastSyncAt)
            .FirstOrDefaultAsync(ct);

        return Ok(new
        {
            success = true,
            organizationId = orgId,
            connection,
            lastSyncAt = lastSync,
            employeesInDatabase = await _context.Employees.CountAsync(e => e.OrganizationId == orgId && !e.IsDeleted, ct)
        });
    }

    [HttpGet("diagnostic")]
    public async Task<IActionResult> Diagnostic(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string provinceCode = "147",
        [FromQuery] string shamsiYearPrefix = "1405",
        [FromQuery] bool applyProvinceFilter = true,
        [FromQuery] bool applyShamsiYearFilter = true,
        [FromQuery] int employeeLimit = 0,
        CancellationToken ct = default)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });
        if (toDate < fromDate)
            return BadRequest(new { success = false, message = "تاریخ پایان باید بعد از تاریخ شروع باشد" });

        var request = new MisSyncDateRangeRequest(fromDate, toDate, provinceCode, shamsiYearPrefix, applyProvinceFilter, applyShamsiYearFilter, employeeLimit);
        var runtimeSettings = _connectionService.BuildForSync(request);
        var range = new MisSyncRange
        {
            SyncFrom = fromDate.Date,
            SyncToExclusive = toDate.Date.AddDays(1),
            Description = $"بازه {fromDate:yyyy-MM-dd} تا {toDate:yyyy-MM-dd}"
        };

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
            hints.Add("اتصال به MIS برقرار نشد. مدیر فنی باید Server و Password را در appsettings تنظیم کند.");
            return hints;
        }

        if (d.TotalInView == 0)
            hints.Add("View خالی است یا کاربر MIS به آن دسترسی ندارد.");
        else if (d.CountAfterSyncFrom == 0)
        {
            var toInclusive = d.SyncTo?.AddDays(-1);
            hints.Add($"هیچ رکوردی در بازه {d.SyncFrom:yyyy-MM-dd} تا {toInclusive:yyyy-MM-dd} نیست. بازه دیگری انتخاب کنید.");
        }
        else if (d.ApplyProvinceFilter && d.CountAfterProvince == 0)
            hints.Add($"کد استان {d.ProvinceCode} با داده‌های MIS مطابقت ندارد.");
        else if (d.ApplyShamsiYearFilter && d.CountAfterShamsiYear == 0)
            hints.Add($"سال شمسی {d.ShamsiYearPrefix} در این بازه یافت نشد.");
        else if (d.CountWithActiveFilters == 0)
            hints.Add("با فیلترهای انتخاب‌شده هیچ رکوردی برنمی‌گردد.");
        else if (d.EmployeesInHrDatabase == 0)
            hints.Add($"MIS دارای {d.DistinctEmployeesWithActiveFilters} پرسنل در این بازه است. دکمه دریافت داده را بزنید.");
        else
            hints.Add($"در سیستم {d.EmployeesInHrDatabase} کارمند ثبت شده است.");

        return hints;
    }

    [HttpGet("records")]
    public async Task<IActionResult> Records(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        return Ok(await _mediator.Send(new GetAttendanceRecordsQuery(orgId, fromDate, toDate), ct));
    }
}

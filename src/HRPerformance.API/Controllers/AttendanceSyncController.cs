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
    private readonly ILogger<AttendanceSyncController> _logger;

    public AttendanceSyncController(
        IAttendanceSyncService syncService,
        ICurrentUserService currentUser,
        MisHrDataReader misHrDataReader,
        HrIntegrationConnectionService connectionService,
        ApplicationDbContext context,
        IMediator mediator,
        ILogger<AttendanceSyncController> logger)
    {
        _syncService = syncService;
        _currentUser = currentUser;
        _misHrDataReader = misHrDataReader;
        _connectionService = connectionService;
        _context = context;
        _mediator = mediator;
        _logger = logger;
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

        try
        {
            var range = MisSyncRequestMapper.ToSyncRange(request);
            var runtimeSettings = _connectionService.BuildForSync(request);
            var queryPreview = _misHrDataReader.BuildQueryPreview(runtimeSettings, range);
            var result = await _syncService.SyncDateRangeAsync(orgId, request, ct);
            return Ok(new
            {
                success = true,
                message = $"داده‌های بازه {range.Description} دریافت شد",
                result,
                misRowsFetched = result.MisRowsFetched,
                shamsiRange = new
                {
                    from = range.ShamsiFromText,
                    to = range.ShamsiToText,
                    fromKey = range.ShamsiFromKey,
                    toKey = range.ShamsiToKey
                },
                queryPreview = new
                {
                    queryPreview.Note,
                    queryPreview.SqlWithLiteralValues,
                    parameters = queryPreview.Parameters.ToDictionary(
                        p => p.Key,
                        p => p.Value is DateTime dt ? dt.ToString("yyyy-MM-dd HH:mm:ss") : p.Value)
                }
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
        var connection = _connectionService.GetStatus();

        if (orgId == Guid.Empty)
        {
            return Ok(new
            {
                success = true,
                organizationId = (Guid?)null,
                connection,
                warning = "شناسه سازمان در حساب کاربری یافت نشد. database/08_SeedData.sql را اجرا کنید.",
                lastSyncAt = (DateTime?)null,
                employeesInDatabase = 0
            });
        }

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

    [HttpGet("preview-query")]
    public IActionResult PreviewQuery(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? shamsiFromYear,
        [FromQuery] string? shamsiFromMonth,
        [FromQuery] string? shamsiFromDay,
        [FromQuery] string? shamsiToYear,
        [FromQuery] string? shamsiToMonth,
        [FromQuery] string? shamsiToDay,
        [FromQuery] string? employeeLimit = null)
    {
        if (!MisShamsiQueryParser.TryParseRange(
                from, to,
                shamsiFromYear, shamsiFromMonth, shamsiFromDay,
                shamsiToYear, shamsiToMonth, shamsiToDay,
                out var request, out var parseError,
                ParseEmployeeLimit(employeeLimit)))
        {
            return BadRequest(new { success = false, message = parseError });
        }

        try
        {
            var range = MisSyncRequestMapper.ToSyncRange(request);
            var runtimeSettings = _connectionService.BuildForSync(request);
            var preview = _misHrDataReader.BuildQueryPreview(runtimeSettings, range);
            if (string.IsNullOrWhiteSpace(preview.SqlWithLiteralValues))
            {
                _logger.LogWarning("Preview-query returned empty SQL for range {Range}", range.Description);
                return BadRequest(new { success = false, message = "کوئری ساخته‌شده خالی است" });
            }

            _logger.LogInformation("Preview-query built for {Range}", range.Description);

            return Ok(new
            {
                success = true,
                apiVersion = "2.8.9-dev",
                shamsiRange =
                    $"{request.ShamsiFromYear}/{request.ShamsiFromMonth:D2}/{request.ShamsiFromDay:D2} تا " +
                    $"{request.ShamsiToYear}/{request.ShamsiToMonth:D2}/{request.ShamsiToDay:D2}",
                shamsiFromKey = preview.ShamsiFromKey,
                shamsiToKey = preview.ShamsiToKey,
                note = preview.Note,
                sql = preview.SqlWithLiteralValues,
                sqlWithParameters = preview.SqlWithParameters,
                parameters = preview.Parameters.ToDictionary(
                    p => p.Key,
                    p => p.Value is DateTime dt ? dt.ToString("yyyy-MM-dd HH:mm:ss") : p.Value)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Preview-query failed for {Range}", request);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("diagnostic")]
    public async Task<IActionResult> Diagnostic(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? shamsiFromYear,
        [FromQuery] string? shamsiFromMonth,
        [FromQuery] string? shamsiFromDay,
        [FromQuery] string? shamsiToYear,
        [FromQuery] string? shamsiToMonth,
        [FromQuery] string? shamsiToDay,
        [FromQuery] string? employeeLimit = null,
        CancellationToken ct = default)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (orgId == Guid.Empty)
            return BadRequest(new { success = false, message = "شناسه سازمان یافت نشد" });

        if (!MisShamsiQueryParser.TryParseRange(
                from, to,
                shamsiFromYear, shamsiFromMonth, shamsiFromDay,
                shamsiToYear, shamsiToMonth, shamsiToDay,
                out var request, out var parseError,
                ParseEmployeeLimit(employeeLimit)))
        {
            return BadRequest(new { success = false, message = parseError });
        }

        MisSyncRange range;
        try
        {
            range = MisSyncRequestMapper.ToSyncRange(request);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }

        var runtimeSettings = _connectionService.BuildForSync(request);

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
            hints.Add($"هیچ رکوردی در بازه شمسی {d.ShamsiFromText} تا {d.ShamsiToText} نیست. بازه دیگری انتخاب کنید.");
        else if (d.CountAfterProvince == 0)
            hints.Add($"گروه پرسنلی {d.ProvinceCode} در این بازه داده‌ای ندارد.");
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
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? shamsiFromYear,
        [FromQuery] string? shamsiFromMonth,
        [FromQuery] string? shamsiFromDay,
        [FromQuery] string? shamsiToYear,
        [FromQuery] string? shamsiToMonth,
        [FromQuery] string? shamsiToDay,
        CancellationToken ct = default)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        if (!MisShamsiQueryParser.TryParseRange(
                from, to,
                shamsiFromYear, shamsiFromMonth, shamsiFromDay,
                shamsiToYear, shamsiToMonth, shamsiToDay,
                out var request, out var parseError))
        {
            return BadRequest(new { success = false, message = parseError });
        }

        var (fromDate, toDate) = MisSyncRequestMapper.ToGregorianRange(request);
        return Ok(await _mediator.Send(new GetAttendanceRecordsQuery(orgId, fromDate, toDate), ct));
    }

    private static int ParseEmployeeLimit(string? raw) =>
        int.TryParse(raw?.Trim(), out var limit) ? Math.Max(0, limit) : 0;
}

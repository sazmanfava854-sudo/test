using HRPerformance.Domain.Models;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Services.ExternalHr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly HrIntegrationConnectionService _misConnection;
    private readonly MisHrDataReader _misReader;

    public HealthController(
        ApplicationDbContext context,
        HrIntegrationConnectionService misConnection,
        MisHrDataReader misReader)
    {
        _context = context;
        _misConnection = misConnection;
        _misReader = misReader;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbHealthy = false;
        string? dbError = null;
        try
        {
            dbHealthy = await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            dbError = ex.GetBaseException().Message;
        }

        return Ok(new
        {
            status = dbHealthy ? "Healthy" : "Degraded",
            database = dbHealthy ? "Connected" : "Disconnected",
            databaseError = dbError,
            hint = dbHealthy
                ? (string?)null
                : "app\\appsettings.Development.json را تنظیم کنید و database/01 تا 11 را اجرا کنید",
            login = "admin / Admin@123",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("mis")]
    public IActionResult MisConnection()
    {
        var status = _misConnection.GetStatus();
        return Ok(new
        {
            isConnectionConfigured = status.IsConnectionConfigured,
            sourceType = status.SourceType,
            missingFields = status.MissingFields,
            server = status.Server,
            database = status.Database,
            userId = status.UserId,
            passwordIsPlaceholder = status.PasswordIsPlaceholder
        });
    }

    /// <summary>تست تبدیل شمسی → میلادی</summary>
    [HttpGet("shamsi-convert")]
    public IActionResult ShamsiConvert([FromQuery] int year, [FromQuery] int month, [FromQuery] int day)
    {
        try
        {
            var gregorian = ShamsiCalendarHelper.ToGregorianDateOnly(year, month, day);
            return Ok(new
            {
                success = true,
                apiVersion = "2.8.7-dev",
                shamsi = $"{year}/{month:D2}/{day:D2}",
                gregorian = gregorian.ToString("yyyy-MM-dd"),
                ok = ShamsiCalendarHelper.IsGregorianDate(gregorian)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>تست زنده اتصال MIS + شمارش رکورد — بدون لاگین</summary>
    [HttpGet("mis-live-test")]
    public async Task<IActionResult> MisLiveTest(
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
        if (!MisShamsiQueryParser.TryParseRange(
                from, to,
                shamsiFromYear, shamsiFromMonth, shamsiFromDay,
                shamsiToYear, shamsiToMonth, shamsiToDay,
                out var request, out var parseError))
        {
            return BadRequest(new { success = false, message = parseError });
        }

        var range = MisSyncRequestMapper.ToSyncRange(request);
        var runtime = _misConnection.BuildForSync(request);
        var status = _misConnection.GetStatus();
        var (canConnect, rowCount, error) = await _misReader.CountRowsAsync(runtime, range, ct);

        return Ok(new
        {
            success = canConnect && string.IsNullOrEmpty(error),
            apiVersion = "2.8.8-dev",
            canConnect = canConnect && status.IsConnectionConfigured,
            isConnectionConfigured = status.IsConnectionConfigured,
            rowCount,
            shamsiRange = range.Description,
            shamsiFromKey = range.ShamsiFromKey,
            shamsiToKey = range.ShamsiToKey,
            server = status.Server,
            database = status.Database,
            error,
            hint = !status.IsConnectionConfigured
                ? "Password/Server در appsettings.Development.json تنظیم نشده"
                : canConnect && string.IsNullOrEmpty(error)
                    ? rowCount > 0
                        ? $"MIS دارای {rowCount} رکورد در این بازه است"
                        : "اتصال OK — در این بازه داده‌ای نیست (بازه دیگر امتحان کنید)"
                    : $"خطای اتصال MIS: {error}"
        });
    }

    /// <summary>
    /// پیش‌نمایش کوئری MIS — بدون نیاز به لاگین (فقط ساخت SQL، بدون اتصال به MIS)
    /// </summary>
    [HttpGet("mis-preview-query")]
    public IActionResult MisPreviewQuery(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? shamsiFromYear,
        [FromQuery] string? shamsiFromMonth,
        [FromQuery] string? shamsiFromDay,
        [FromQuery] string? shamsiToYear,
        [FromQuery] string? shamsiToMonth,
        [FromQuery] string? shamsiToDay)
    {
        if (!MisShamsiQueryParser.TryParseRange(
                from, to,
                shamsiFromYear, shamsiFromMonth, shamsiFromDay,
                shamsiToYear, shamsiToMonth, shamsiToDay,
                out var request, out var parseError))
        {
            return BadRequest(new
            {
                success = false,
                message = parseError,
                example =
                    "/api/health/mis-preview-query?from=1404/04/10&to=1404/04/11",
                exampleAlt =
                    "/api/health/mis-preview-query?shamsiFromYear=1404&shamsiFromMonth=4&shamsiFromDay=10&shamsiToYear=1404&shamsiToMonth=4&shamsiToDay=11"
            });
        }

        try
        {
            var range = MisSyncRequestMapper.ToSyncRange(request);
            var settings = new HrIntegrationRuntimeSettings
            {
                ProvinceCode = MisSyncDefaults.PersonnelGroupCode,
                ApplyProvinceFilter = true,
                ApplyShamsiYearFilter = false
            };
            var preview = MisQueryBuilder.BuildPreview(settings, range);

            return Ok(new
            {
                success = true,
                apiVersion = "2.8.7-dev",
                shamsiRange =
                    $"{request.ShamsiFromYear}/{request.ShamsiFromMonth:D2}/{request.ShamsiFromDay:D2} تا " +
                    $"{request.ShamsiToYear}/{request.ShamsiToMonth:D2}/{request.ShamsiToDay:D2}",
                shamsiFromKey = preview.ShamsiFromKey,
                shamsiToKey = preview.ShamsiToKey,
                sql = preview.SqlWithLiteralValues,
                note = preview.Note
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message,
                hint = "تاریخ باید شمسی باشد — from=1404/04/10&to=1404/04/11",
                example =
                    "/api/health/mis-preview-query?from=1404/04/10&to=1404/04/11"
            });
        }
    }
}

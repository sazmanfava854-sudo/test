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

    public HealthController(ApplicationDbContext context, HrIntegrationConnectionService misConnection)
    {
        _context = context;
        _misConnection = misConnection;
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

    /// <summary>
    /// پیش‌نمایش کوئری MIS — بدون نیاز به لاگین (فقط ساخت SQL، بدون اتصال به MIS)
    /// </summary>
    [HttpGet("mis-preview-query")]
    public IActionResult MisPreviewQuery(
        [FromQuery] int shamsiFromYear,
        [FromQuery] int shamsiFromMonth,
        [FromQuery] int shamsiFromDay,
        [FromQuery] int shamsiToYear,
        [FromQuery] int shamsiToMonth,
        [FromQuery] int shamsiToDay)
    {
        try
        {
            var request = new MisSyncDateRangeRequest(
                shamsiFromYear, shamsiFromMonth, shamsiFromDay,
                shamsiToYear, shamsiToMonth, shamsiToDay);
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
                apiVersion = "2.8.6-dev",
                shamsiRange =
                    $"{shamsiFromYear}/{shamsiFromMonth:D2}/{shamsiFromDay:D2} تا " +
                    $"{shamsiToYear}/{shamsiToMonth:D2}/{shamsiToDay:D2}",
                sql = preview.SqlWithLiteralValues,
                gregorianRange = new
                {
                    from = preview.GregorianFrom.ToString("yyyy-MM-dd HH:mm:ss"),
                    to = preview.GregorianToInclusive.ToString("yyyy-MM-dd HH:mm:ss")
                },
                conversionOk = preview.GregorianFrom.Year is >= 1990 and <= 2100
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

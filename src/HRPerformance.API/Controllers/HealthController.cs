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
}

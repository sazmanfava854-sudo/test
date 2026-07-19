using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRPerformance.Infrastructure.Data;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public HealthController(ApplicationDbContext context) => _context = context;

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
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRPerformance.Infrastructure.Data;

namespace HRPerformance.API.Controllers;
[ApiController] [Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public HealthController(ApplicationDbContext context) => _context = context;
    [HttpGet] public async Task<IActionResult> Get()
    {
        var dbHealthy = false;
        try { dbHealthy = await _context.Database.CanConnectAsync(); } catch { }
        return Ok(new { status = dbHealthy ? "Healthy" : "Degraded", database = dbHealthy ? "Connected" : "Disconnected", timestamp = DateTime.UtcNow });
    }
}

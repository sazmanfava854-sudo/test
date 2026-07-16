using HRPerformance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
public class AttendanceSyncController : ControllerBase
{
    private readonly IAttendanceSyncService _syncService;
    private readonly ICurrentUserService _currentUser;

    public AttendanceSyncController(IAttendanceSyncService syncService, ICurrentUserService currentUser)
    {
        _syncService = syncService;
        _currentUser = currentUser;
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
}

using HRPerformance.DTOs.Auth;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) =>
        Ok(await _authService.LoginAsync(request));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request) =>
        Ok(await _authService.RefreshAsync(request));

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { User.Identity?.Name });
}

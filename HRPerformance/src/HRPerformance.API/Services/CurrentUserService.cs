using System.Security.Claims;
using HRPerformance.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRPerformance.Services;
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;
    public CurrentUserService(IHttpContextAccessor http) => _http = http;
    private ClaimsPrincipal? User => _http.HttpContext?.User;
    public Guid? UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);
    public Guid? OrganizationId => Guid.TryParse(User?.FindFirstValue("organizationId"), out var id) ? id : null;
    public Guid? EmployeeId => Guid.TryParse(User?.FindFirstValue("employeeId"), out var id) ? id : null;
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}

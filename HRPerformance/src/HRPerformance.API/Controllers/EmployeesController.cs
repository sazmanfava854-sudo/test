using HRPerformance.DTOs.Employees;
using HRPerformance.Interfaces;
using HRPerformance.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPerformance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly ICurrentUserService _currentUser;

    public EmployeesController(EmployeeService employeeService, ICurrentUserService currentUser)
    {
        _employeeService = employeeService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeSearchRequest request) =>
        Ok(await _employeeService.GetAllAsync(request, _currentUser.OrganizationId ?? Guid.Empty));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _employeeService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request) =>
        Ok(await _employeeService.CreateAsync(request, _currentUser.OrganizationId ?? Guid.Empty));

    [HttpPut]
    [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator,Manager")]
    public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest request) =>
        Ok(await _employeeService.UpdateAsync(request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "OrganizationAdministrator,SuperAdministrator")]
    public async Task<IActionResult> Delete(Guid id) =>
        Ok(await _employeeService.DeleteAsync(id));
}

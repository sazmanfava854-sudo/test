using HRPerformance.Entities;
using HRPerformance.Enums;
namespace HRPerformance.Interfaces;
public interface ICurrentUserService
{
    Guid? UserId { get; } string? UserName { get; } Guid? OrganizationId { get; } Guid? EmployeeId { get; } bool IsInRole(string role);
}

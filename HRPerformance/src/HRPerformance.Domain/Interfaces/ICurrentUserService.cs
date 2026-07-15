using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
namespace HRPerformance.Domain.Interfaces;
public interface ICurrentUserService
{
    Guid? UserId { get; } string? UserName { get; } Guid? OrganizationId { get; } Guid? EmployeeId { get; } bool IsInRole(string role);
}

namespace HRPerformance.Application.Interfaces;

public interface IEmployeeAccountLinkService
{
    /// <summary>
    /// Returns linked EmployeeId; auto-links Users.EmployeeId when UserName matches PersonnelCode.
    /// </summary>
    Task<Guid?> ResolveEmployeeIdAsync(Guid userId, string? userName, Guid? organizationId, CancellationToken ct = default);
}

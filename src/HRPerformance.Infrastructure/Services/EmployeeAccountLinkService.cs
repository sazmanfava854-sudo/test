using HRPerformance.Application.Interfaces;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Infrastructure.Services;

public class EmployeeAccountLinkService : IEmployeeAccountLinkService
{
    private readonly ApplicationDbContext _context;

    public EmployeeAccountLinkService(ApplicationDbContext context) => _context = context;

    public async Task<Guid?> ResolveEmployeeIdAsync(
        Guid userId,
        string? userName,
        Guid? organizationId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return null;

        var user = await _context.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return null;

        if (user.EmployeeId.HasValue && user.EmployeeId.Value != Guid.Empty)
            return user.EmployeeId;

        var orgId = organizationId ?? user.OrganizationId;
        if (!orgId.HasValue || string.IsNullOrWhiteSpace(userName))
            return null;

        var personnelCode = userName.Trim();
        var employee = await _context.Employees
            .AsTracking()
            .FirstOrDefaultAsync(
                e => e.OrganizationId == orgId.Value
                     && e.PersonnelCode == personnelCode
                     && !e.IsDeleted,
                ct);

        if (employee == null)
            return null;

        user.EmployeeId = employee.Id;
        if (!employee.UserId.HasValue)
            employee.UserId = user.Id;

        await _context.SaveChangesAsync(ct);
        return employee.Id;
    }
}

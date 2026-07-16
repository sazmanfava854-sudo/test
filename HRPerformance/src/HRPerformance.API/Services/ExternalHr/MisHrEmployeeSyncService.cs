using HRPerformance.Entities;
using HRPerformance.Enums;
using HRPerformance.Data;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.ExternalHr;

public class MisHrEmployeeSyncService
{
    private readonly ApplicationDbContext _context;

    public MisHrEmployeeSyncService(ApplicationDbContext context) => _context = context;

    public async Task<Employee> UpsertEmployeeAsync(Guid organizationId, MisHourlyLeaveRecord record, CancellationToken ct)
    {
        var personnelCode = record.PerCod.Trim();
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.OrganizationId == organizationId && e.PersonnelCode == personnelCode && !e.IsDeleted, ct);

        if (employee == null)
        {
            employee = new Employee
            {
                OrganizationId = organizationId,
                PersonnelCode = personnelCode,
                NationalCode = NormalizeNationalCode(record.NationalIDNo, personnelCode),
                FirstName = record.Name ?? "نامشخص",
                LastName = record.LastName ?? "نامشخص",
                EmploymentDate = record.StartDate.Date,
                EmploymentType = EmploymentType.Permanent,
                Status = EmployeeStatus.Active,
                Description = $"سینک از MIS - ProvinceCode: {record.ProvinceCode}"
            };
            _context.Employees.Add(employee);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(record.Name)) employee.FirstName = record.Name.Trim();
            if (!string.IsNullOrWhiteSpace(record.LastName)) employee.LastName = record.LastName.Trim();
            if (!string.IsNullOrWhiteSpace(record.NationalIDNo)) employee.NationalCode = record.NationalIDNo.Trim();
            employee.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return employee;
    }

    private static string NormalizeNationalCode(string? nationalId, string personnelCode)
    {
        if (!string.IsNullOrWhiteSpace(nationalId)) return nationalId.Trim();
        return personnelCode.PadLeft(10, '0')[..10];
    }
}

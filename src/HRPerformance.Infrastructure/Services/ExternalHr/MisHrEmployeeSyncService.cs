using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

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

    public async Task<int> SyncDistinctEmployeesAsync(
        Guid organizationId,
        IReadOnlyList<MisHourlyLeaveRecord> records,
        CancellationToken ct = default)
    {
        if (records.Count == 0)
            return 0;

        var personnelCodes = records
            .Select(r => r.PerCod.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existing = await _context.Employees
            .Where(e => e.OrganizationId == organizationId && !e.IsDeleted && personnelCodes.Contains(e.PersonnelCode))
            .ToListAsync(ct);

        var existingByCode = existing.ToDictionary(e => e.PersonnelCode, StringComparer.OrdinalIgnoreCase);
        var upserted = 0;

        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.PerCod))
                continue;

            var personnelCode = record.PerCod.Trim();
            if (!existingByCode.TryGetValue(personnelCode, out var employee))
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
                existingByCode[personnelCode] = employee;
                upserted++;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(record.Name)) employee.FirstName = record.Name.Trim();
                if (!string.IsNullOrWhiteSpace(record.LastName)) employee.LastName = record.LastName.Trim();
                if (!string.IsNullOrWhiteSpace(record.NationalIDNo)) employee.NationalCode = record.NationalIDNo.Trim();
                employee.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);
        return upserted;
    }

    private static string NormalizeNationalCode(string? nationalId, string personnelCode)
    {
        if (!string.IsNullOrWhiteSpace(nationalId)) return nationalId.Trim();
        return personnelCode.PadLeft(10, '0')[..10];
    }
}

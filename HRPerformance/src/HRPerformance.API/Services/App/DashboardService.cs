using HRPerformance.Common;
using HRPerformance.DTOs.Dashboard;
using HRPerformance.Entities;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.App;

public class DashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<EmployeeDashboardDto>> GetEmployeeDashboardAsync(Guid employeeId, CancellationToken ct = default)
    {
        var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId, ct);
        if (emp == null) return ApiResponse<EmployeeDashboardDto>.Fail("کارمند یافت نشد");

        var scores = await _uow.Repository<EmployeeScore>().Query()
            .Where(s => s.EmployeeId == employeeId).OrderByDescending(s => s.ScoreDate).Take(12).ToListAsync(ct);
        var attendance = await _uow.Repository<AttendanceLog>().Query()
            .Where(a => a.EmployeeId == employeeId).OrderByDescending(a => a.AttendanceDate).Take(30).ToListAsync(ct);
        var trend = scores.GroupBy(s => $"{s.Year}/{s.Month}").Select(g => new ScoreTrendDto(g.Key, g.Sum(x => x.Score))).ToList();
        var attSummary = attendance.Select(a => new AttendanceSummaryDto(a.AttendanceDate, !a.IsAbsent, a.DelayMinutes, a.IsAbsent)).ToList();
        var pos = scores.Count(s => s.ScoreType == ScoreType.Positive);
        var neg = scores.Count(s => s.ScoreType == ScoreType.Negative);
        return ApiResponse<EmployeeDashboardDto>.Ok(new EmployeeDashboardDto(emp.CurrentScore, emp.MonthlyScore, emp.YearlyScore, emp.Ranking, trend, attSummary, pos, neg));
    }

    public async Task<ApiResponse<ManagerDashboardDto>> GetManagerDashboardAsync(Guid managerId, Guid organizationId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var emps = await _uow.Repository<Employee>().Query()
            .Include(e => e.OrganizationUnit)
            .Where(e => e.ManagerId == managerId && !e.IsDeleted).ToListAsync(ct);
        var empIds = emps.Select(e => e.Id).ToList();
        var att = await _uow.Repository<AttendanceLog>().Query()
            .Where(a => empIds.Contains(a.EmployeeId) && a.AttendanceDate == today).ToListAsync(ct);
        var top = emps.OrderByDescending(e => e.CurrentScore).Take(5)
            .Select(e => new TopEmployeeDto(e.Id, e.FullName, e.OrganizationUnit?.Name, e.CurrentScore, e.Ranking)).ToList();
        var weak = emps.OrderBy(e => e.CurrentScore).Take(5)
            .Select(e => new TopEmployeeDto(e.Id, e.FullName, e.OrganizationUnit?.Name, e.CurrentScore, e.Ranking)).ToList();
        var avg = emps.Any() ? emps.Average(e => e.CurrentScore) : 0;
        return ApiResponse<ManagerDashboardDto>.Ok(new ManagerDashboardDto(emps.Count, att.Count(a => !a.IsAbsent), att.Count(a => a.DelayMinutes > 0), att.Count(a => a.IsAbsent), avg, top, weak, new List<ChartDataDto>()));
    }

    public async Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync(Guid organizationId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var emps = await _uow.Repository<Employee>().Query()
            .Where(e => e.OrganizationId == organizationId && !e.IsDeleted).ToListAsync(ct);
        var depts = await _uow.Repository<OrganizationUnit>().Query()
            .Where(u => u.OrganizationId == organizationId && !u.IsDeleted).CountAsync(ct);
        var att = await _uow.Repository<AttendanceLog>().Query()
            .Where(a => a.OrganizationId == organizationId && a.AttendanceDate == today).ToListAsync(ct);
        var mgrCount = emps.Count(e => emps.Any(s => s.ManagerId == e.Id));
        var deptRanks = await _uow.Repository<OrganizationUnit>().Query()
            .Where(u => u.OrganizationId == organizationId && !u.IsDeleted)
            .Select(u => new DepartmentRankDto(u.Id, u.Name,
                emps.Where(e => e.OrganizationUnitId == u.Id).Average(e => (decimal?)e.CurrentScore) ?? 0,
                emps.Count(e => e.OrganizationUnitId == u.Id))).ToListAsync(ct);
        return ApiResponse<AdminDashboardDto>.Ok(new AdminDashboardDto(emps.Count, mgrCount, depts, att.Count(a => !a.IsAbsent), att.Count(a => a.IsAbsent), emps.Any() ? emps.Average(e => e.CurrentScore) : 0, deptRanks, new List<ChartDataDto>()));
    }
}

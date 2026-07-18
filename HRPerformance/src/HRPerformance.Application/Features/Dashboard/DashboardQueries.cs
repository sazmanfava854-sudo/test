using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Dashboard;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Dashboard;
public record GetEmployeeDashboardQuery(Guid EmployeeId) : IRequest<ApiResponse<EmployeeDashboardDto>>;
public record GetManagerDashboardQuery(Guid ManagerId, Guid OrganizationId) : IRequest<ApiResponse<ManagerDashboardDto>>;
public record GetAdminDashboardQuery(Guid OrganizationId) : IRequest<ApiResponse<AdminDashboardDto>>;

public class GetEmployeeDashboardQueryHandler : IRequestHandler<GetEmployeeDashboardQuery, ApiResponse<EmployeeDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    public GetEmployeeDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<EmployeeDashboardDto>> Handle(GetEmployeeDashboardQuery q, CancellationToken ct)
    {
        var emp = await _uow.Repository<Employee>().GetByIdAsync(q.EmployeeId, ct);
        if (emp == null) return ApiResponse<EmployeeDashboardDto>.Fail("کارمند یافت نشد");
        var scores = await _uow.Repository<EmployeeScore>().Query().Where(s => s.EmployeeId == q.EmployeeId).OrderByDescending(s => s.ScoreDate).Take(12).ToListAsync(ct);
        var attendance = await _uow.Repository<AttendanceLog>().Query().Where(a => a.EmployeeId == q.EmployeeId).OrderByDescending(a => a.AttendanceDate).Take(30).ToListAsync(ct);
        var trend = scores.GroupBy(s => $"{s.Year}/{s.Month}").Select(g => new ScoreTrendDto(g.Key, g.Sum(x => x.Score))).ToList();
        var attSummary = attendance.Select(a => new AttendanceSummaryDto(a.AttendanceDate, !a.IsAbsent, a.DelayMinutes, a.IsAbsent)).ToList();
        var pos = scores.Count(s => s.ScoreType == Domain.Enums.ScoreType.Positive);
        var neg = scores.Count(s => s.ScoreType == Domain.Enums.ScoreType.Negative);
        return ApiResponse<EmployeeDashboardDto>.Ok(new EmployeeDashboardDto(emp.CurrentScore, emp.MonthlyScore, emp.YearlyScore, emp.Ranking, trend, attSummary, pos, neg));
    }
}

public class GetManagerDashboardQueryHandler : IRequestHandler<GetManagerDashboardQuery, ApiResponse<ManagerDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    public GetManagerDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<ManagerDashboardDto>> Handle(GetManagerDashboardQuery q, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var emps = await _uow.Repository<Employee>().Query().Where(e => e.ManagerId == q.ManagerId && !e.IsDeleted).ToListAsync(ct);
        var empIds = emps.Select(e => e.Id).ToList();
        var att = await _uow.Repository<AttendanceLog>().Query().Where(a => empIds.Contains(a.EmployeeId) && a.AttendanceDate == today).ToListAsync(ct);
        var top = emps.OrderByDescending(e => e.CurrentScore).Take(5).Select(e => new TopEmployeeDto(e.Id, e.FullName, e.OrganizationUnit?.Name, e.CurrentScore, e.Ranking)).ToList();
        var weak = emps.OrderBy(e => e.CurrentScore).Take(5).Select(e => new TopEmployeeDto(e.Id, e.FullName, e.OrganizationUnit?.Name, e.CurrentScore, e.Ranking)).ToList();
        var avg = emps.Any() ? emps.Average(e => e.CurrentScore) : 0;
        return ApiResponse<ManagerDashboardDto>.Ok(new ManagerDashboardDto(emps.Count, att.Count(a => !a.IsAbsent), att.Count(a => a.DelayMinutes > 0), att.Count(a => a.IsAbsent), avg, top, weak, new List<ChartDataDto>()));
    }
}

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, ApiResponse<AdminDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAdminDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<AdminDashboardDto>> Handle(GetAdminDashboardQuery q, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var emps = await _uow.Repository<Employee>().Query().Where(e => e.OrganizationId == q.OrganizationId && !e.IsDeleted).ToListAsync(ct);
        var depts = await _uow.Repository<OrganizationUnit>().Query().Where(u => u.OrganizationId == q.OrganizationId && !u.IsDeleted).CountAsync(ct);
        var att = await _uow.Repository<AttendanceLog>().Query().Where(a => a.OrganizationId == q.OrganizationId && a.AttendanceDate == today).ToListAsync(ct);
        var mgrCount = emps.Count(e => emps.Any(s => s.ManagerId == e.Id));
        var deptRanks = await _uow.Repository<OrganizationUnit>().Query().Where(u => u.OrganizationId == q.OrganizationId && !u.IsDeleted)
            .Select(u => new DepartmentRankDto(u.Id, u.Name, emps.Where(e => e.OrganizationUnitId == u.Id).Average(e => (decimal?)e.CurrentScore) ?? 0, emps.Count(e => e.OrganizationUnitId == u.Id))).ToListAsync(ct);
        return ApiResponse<AdminDashboardDto>.Ok(new AdminDashboardDto(emps.Count, mgrCount, depts, att.Count(a => !a.IsAbsent), att.Count(a => a.IsAbsent), emps.Any() ? emps.Average(e => e.CurrentScore) : 0, deptRanks, new List<ChartDataDto>()));
    }
}

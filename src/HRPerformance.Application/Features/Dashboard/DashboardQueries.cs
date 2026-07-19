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
public record GetAttendanceRecordsQuery(Guid OrganizationId, DateTime FromDate, DateTime ToDate) : IRequest<ApiResponse<IList<AttendanceRecordDto>>>;

public class GetEmployeeDashboardQueryHandler : IRequestHandler<GetEmployeeDashboardQuery, ApiResponse<EmployeeDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    public GetEmployeeDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<EmployeeDashboardDto>> Handle(GetEmployeeDashboardQuery q, CancellationToken ct)
    {
        if (q.EmployeeId == Guid.Empty)
            return ApiResponse<EmployeeDashboardDto>.Fail("پروفایل کارمند به حساب کاربری متصل نیست. از داشبورد مدیر یا سازمان استفاده کنید.");

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
        if (q.OrganizationId == Guid.Empty)
            return ApiResponse<ManagerDashboardDto>.Fail("شناسه سازمان یافت نشد");

        var today = DateTime.UtcNow.Date;
        var emps = await _uow.Repository<Employee>().Query()
            .Include(e => e.OrganizationUnit)
            .Where(e => e.OrganizationId == q.OrganizationId && !e.IsDeleted)
            .ToListAsync(ct);

        var empIds = emps.Select(e => e.Id).ToList();
        var att = empIds.Count == 0
            ? new List<AttendanceLog>()
            : await _uow.Repository<AttendanceLog>().Query()
                .Where(a => empIds.Contains(a.EmployeeId) && a.AttendanceDate == today)
                .ToListAsync(ct);

        var top = emps.OrderByDescending(e => e.CurrentScore).Take(5)
            .Select(e => new TopEmployeeDto(e.Id, e.FullName, e.OrganizationUnit?.Name, e.CurrentScore, e.Ranking)).ToList();
        var weak = emps.OrderBy(e => e.CurrentScore).Take(5)
            .Select(e => new TopEmployeeDto(e.Id, e.FullName, e.OrganizationUnit?.Name, e.CurrentScore, e.Ranking)).ToList();
        var avg = emps.Any() ? emps.Average(e => e.CurrentScore) : 0;

        var monthlyTrend = await BuildMonthlyTrendAsync(empIds, ct);
        var teamIndicators = await BuildTeamIndicatorsAsync(q.OrganizationId, emps, att, ct);

        return ApiResponse<ManagerDashboardDto>.Ok(new ManagerDashboardDto(
            emps.Count,
            att.Count(a => !a.IsAbsent && !a.IsOnLeave),
            att.Count(a => a.DelayMinutes > 0),
            att.Count(a => a.IsAbsent),
            avg,
            top,
            weak,
            monthlyTrend,
            teamIndicators));
    }

    private async Task<List<ChartDataDto>> BuildMonthlyTrendAsync(List<Guid> empIds, CancellationToken ct)
    {
        if (empIds.Count == 0) return new List<ChartDataDto>();

        var since = DateTime.UtcNow.Date.AddMonths(-5);
        var scores = await _uow.Repository<EmployeeScore>().Query()
            .Where(s => empIds.Contains(s.EmployeeId) && s.ScoreDate >= since)
            .ToListAsync(ct);

        return scores
            .GroupBy(s => new { s.Year, s.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ChartDataDto($"{g.Key.Year}/{g.Key.Month}", g.Average(x => x.Score)))
            .ToList();
    }

    private async Task<List<ChartDataDto>> BuildTeamIndicatorsAsync(
        Guid organizationId,
        List<Employee> emps,
        List<AttendanceLog> todayAttendance,
        CancellationToken ct)
    {
        var categories = await _uow.Repository<EvaluationCategory>().Query()
            .Where(c => c.OrganizationId == organizationId && !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        if (categories.Count == 0)
        {
            return new List<ChartDataDto>
            {
                new("کیفیت", emps.Any() ? emps.Average(e => e.CurrentScore) : 0),
                new("حضور", CalcAttendanceScore(emps.Count, todayAttendance)),
                new("انضباط", 0),
                new("کار تیمی", 0),
                new("بهره‌وری", 0)
            };
        }

        var empIds = emps.Select(e => e.Id).ToList();
        var scores = await _uow.Repository<EmployeeScore>().Query()
            .Where(s => empIds.Contains(s.EmployeeId) && s.CategoryId != null)
            .ToListAsync(ct);

        var indicators = new List<ChartDataDto>();
        foreach (var cat in categories)
        {
            var catScores = scores.Where(s => s.CategoryId == cat.Id).ToList();
            decimal value;
            if (catScores.Count > 0)
                value = catScores.Average(s => s.Score);
            else if (cat.Name.Contains("حضور", StringComparison.OrdinalIgnoreCase))
                value = CalcAttendanceScore(emps.Count, todayAttendance);
            else
                value = emps.Any() ? emps.Average(e => e.CurrentScore) * (cat.Weight / 100m) : 0;

            indicators.Add(new ChartDataDto(cat.Name, Math.Round(value, 1)));
        }

        return indicators;
    }

    private static decimal CalcAttendanceScore(int employeeCount, List<AttendanceLog> todayAttendance)
    {
        if (employeeCount == 0) return 0;
        var present = todayAttendance.Count(a => !a.IsAbsent);
        return Math.Round((decimal)present / employeeCount * 100m, 1);
    }
}

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, ApiResponse<AdminDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAdminDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<AdminDashboardDto>> Handle(GetAdminDashboardQuery q, CancellationToken ct)
    {
        if (q.OrganizationId == Guid.Empty)
            return ApiResponse<AdminDashboardDto>.Fail("شناسه سازمان یافت نشد");

        var today = DateTime.UtcNow.Date;
        var emps = await _uow.Repository<Employee>().Query()
            .Where(e => e.OrganizationId == q.OrganizationId && !e.IsDeleted)
            .ToListAsync(ct);
        var units = await _uow.Repository<OrganizationUnit>().Query()
            .Where(u => u.OrganizationId == q.OrganizationId && !u.IsDeleted)
            .ToListAsync(ct);
        var att = await _uow.Repository<AttendanceLog>().Query()
            .Where(a => a.OrganizationId == q.OrganizationId && a.AttendanceDate == today)
            .ToListAsync(ct);

        var mgrCount = emps.Count(e => emps.Any(s => s.ManagerId == e.Id));
        var deptRanks = units.Select(u => new DepartmentRankDto(
            u.Id,
            u.Name,
            emps.Where(e => e.OrganizationUnitId == u.Id).DefaultIfEmpty().Average(e => e?.CurrentScore ?? 0),
            emps.Count(e => e.OrganizationUnitId == u.Id))).ToList();

        var distribution = emps
            .GroupBy(e => e.CurrentScore switch
            {
                >= 80 => "عالی",
                >= 60 => "خوب",
                >= 40 => "متوسط",
                _ => "ضعیف"
            })
            .Select(g => new ChartDataDto(g.Key, g.Count()))
            .ToList();

        return ApiResponse<AdminDashboardDto>.Ok(new AdminDashboardDto(
            emps.Count,
            mgrCount,
            units.Count,
            att.Count(a => !a.IsAbsent),
            att.Count(a => a.IsAbsent),
            emps.Any() ? emps.Average(e => e.CurrentScore) : 0,
            deptRanks,
            distribution));
    }
}

public class GetAttendanceRecordsQueryHandler : IRequestHandler<GetAttendanceRecordsQuery, ApiResponse<IList<AttendanceRecordDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetAttendanceRecordsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IList<AttendanceRecordDto>>> Handle(GetAttendanceRecordsQuery q, CancellationToken ct)
    {
        if (q.OrganizationId == Guid.Empty)
            return ApiResponse<IList<AttendanceRecordDto>>.Fail("شناسه سازمان یافت نشد");

        var from = q.FromDate.Date;
        var to = q.ToDate.Date;

        var records = await _uow.Repository<AttendanceLog>().Query()
            .Include(a => a.Employee)
            .Where(a => a.OrganizationId == q.OrganizationId && a.AttendanceDate >= from && a.AttendanceDate <= to)
            .OrderByDescending(a => a.AttendanceDate)
            .ThenBy(a => a.Employee!.LastName)
            .Select(a => new AttendanceRecordDto(
                a.Id,
                a.EmployeeId,
                a.Employee!.PersonnelCode,
                a.Employee.FirstName + " " + a.Employee.LastName,
                a.AttendanceDate,
                a.EntryTime.HasValue ? a.EntryTime.Value.ToString(@"hh\:mm") : null,
                a.ExitTime.HasValue ? a.ExitTime.Value.ToString(@"hh\:mm") : null,
                a.WorkingHours,
                a.IsOnLeave,
                a.LeaveType,
                a.Source))
            .ToListAsync(ct);

        return ApiResponse<IList<AttendanceRecordDto>>.Ok(records);
    }
}

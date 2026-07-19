using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Infrastructure.Services;
public class RankingService : IRankingService
{
    private readonly ApplicationDbContext _context;
    public RankingService(ApplicationDbContext context) => _context = context;

    public async Task CalculateRankingsAsync(Guid organizationId, int year, int? month = null, CancellationToken ct = default)
    {
        var employees = await _context.Employees.Where(e => e.OrganizationId == organizationId && !e.IsDeleted && e.Status == EmployeeStatus.Active).ToListAsync(ct);
        var scores = await _context.EmployeeScores.Where(s => s.OrganizationId == organizationId && s.Year == year && (month == null || s.Month == month)).ToListAsync(ct);
        var ranked = employees.Select(e => new { e.Id, Total = scores.Where(s => s.EmployeeId == e.Id).Sum(s => s.Score) }).OrderByDescending(x => x.Total).ToList();
        _context.Rankings.RemoveRange(_context.Rankings.Where(r => r.OrganizationId == organizationId && r.EntityType == "Employee" && r.Year == year && r.Month == month));
        int rank = 1;
        foreach (var item in ranked)
        {
            _context.Rankings.Add(new Ranking { OrganizationId = organizationId, EntityType = "Employee", EntityId = item.Id, Rank = rank++, Score = item.Total, Year = year, Month = month, PeriodType = month.HasValue ? PeriodType.Monthly : PeriodType.Yearly });
            var emp = employees.First(e => e.Id == item.Id);
            emp.Ranking = rank - 1;
        }
        await _context.SaveChangesAsync(ct);
    }
}

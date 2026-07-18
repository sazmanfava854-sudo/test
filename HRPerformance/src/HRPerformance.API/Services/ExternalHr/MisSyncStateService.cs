using HRPerformance.Data;
using HRPerformance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Services.ExternalHr;

public class MisSyncStateService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MisSyncStateService> _logger;

    public MisSyncStateService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<MisSyncStateService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public string SyncMode => _configuration["HrIntegration:SyncMode"] ?? "Monthly";

    public async Task<HrMisSyncState> GetOrCreateStateAsync(Guid organizationId, CancellationToken ct = default)
    {
        var state = await _context.HrMisSyncStates
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, ct);

        if (state != null)
            return state;

        var targetYear = int.Parse(_configuration["HrIntegration:ShamsiYearPrefix"] ?? "1404");
        var monthsBack = Math.Max(1, _configuration.GetValue("HrIntegration:InitialSyncMonthsBack", 12));
        var (currentYear, currentMonth) = ShamsiDateHelper.GetCurrentShamsi();

        var startMonth = currentYear == targetYear
            ? currentMonth
            : 12;

        var backfillStart = Math.Max(1, startMonth - monthsBack + 1);

        state = new HrMisSyncState
        {
            OrganizationId = organizationId,
            TargetShamsiYear = targetYear,
            NextShamsiMonth = startMonth,
            BackfillStartMonth = backfillStart,
            IsBackfillComplete = false
        };

        _context.HrMisSyncStates.Add(state);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Initialized MIS sync state for org {OrgId}: year={Year}, startMonth={StartMonth}, backfillFrom={BackfillFrom}",
            organizationId, targetYear, startMonth, backfillStart);

        return state;
    }

    public async Task<IReadOnlyList<MisSyncRange>> GetNextRangesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var mode = SyncMode.Trim();
        if (mode.Equals("DaysBack", StringComparison.OrdinalIgnoreCase))
            return [CreateDaysBackRange()];

        if (mode.Equals("DateRange", StringComparison.OrdinalIgnoreCase))
        {
            var from = _configuration.GetValue<DateTime?>("HrIntegration:SyncFromDate");
            var to = _configuration.GetValue<DateTime?>("HrIntegration:SyncToDate");
            if (from.HasValue && to.HasValue)
            {
                return
                [
                    new MisSyncRange
                    {
                        SyncFrom = from.Value.Date,
                        SyncToExclusive = to.Value.Date.AddDays(1),
                        Description = $"بازه {from:yyyy-MM-dd} تا {to:yyyy-MM-dd}"
                    }
                ];
            }
        }

        return await GetMonthlyRangesAsync(organizationId, ct);
    }

    public async Task MarkRangeCompletedAsync(Guid organizationId, MisSyncRange range, CancellationToken ct = default)
    {
        if (!range.ShamsiYear.HasValue || !range.ShamsiMonth.HasValue)
            return;

        var state = await GetOrCreateStateAsync(organizationId, ct);
        if (state.IsBackfillComplete)
        {
            state.LastSyncedAt = DateTime.UtcNow;
            state.LastSyncDescription = range.Description;
            state.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return;
        }

        var nextMonth = state.NextShamsiMonth - 1;
        if (nextMonth < state.BackfillStartMonth)
        {
            state.IsBackfillComplete = true;
            state.LastSyncDescription = $"بک‌فیل کامل شد تا {state.TargetShamsiYear}/{state.BackfillStartMonth:00}";
            _logger.LogInformation("MIS backfill complete for org {OrgId}", organizationId);
        }
        else
        {
            state.NextShamsiMonth = nextMonth;
            state.LastSyncDescription = range.Description;
        }

        state.LastSyncedAt = DateTime.UtcNow;
        state.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<MisSyncRange>> GetMonthlyRangesAsync(Guid organizationId, CancellationToken ct)
    {
        var state = await GetOrCreateStateAsync(organizationId, ct);
        var monthsPerRun = Math.Max(1, _configuration.GetValue("HrIntegration:MonthsPerSyncRun", 1));
        var ranges = new List<MisSyncRange>();

        if (state.IsBackfillComplete)
        {
            var (currentYear, currentMonth) = ShamsiDateHelper.GetCurrentShamsi();
            var month = currentYear == state.TargetShamsiYear ? currentMonth : 12;
            ranges.Add(CreateMonthRange(state.TargetShamsiYear, month, isBackfill: false));
            return ranges;
        }

        var cursorMonth = state.NextShamsiMonth;
        for (var i = 0; i < monthsPerRun && cursorMonth >= state.BackfillStartMonth; i++)
        {
            ranges.Add(CreateMonthRange(state.TargetShamsiYear, cursorMonth, isBackfill: true));
            cursorMonth--;
        }

        return ranges;
    }

    private MisSyncRange CreateDaysBackRange()
    {
        var syncDaysBack = Math.Max(1, _configuration.GetValue("HrIntegration:SyncDaysBack", 30));
        var syncFrom = DateTime.Today.AddDays(-syncDaysBack);
        return new MisSyncRange
        {
            SyncFrom = syncFrom,
            SyncToExclusive = DateTime.Today.AddDays(1),
            Description = $"{syncDaysBack} روز اخیر"
        };
    }

    private static MisSyncRange CreateMonthRange(int shamsiYear, int shamsiMonth, bool isBackfill)
    {
        var (start, endExclusive) = ShamsiDateHelper.GetGregorianMonthRange(shamsiYear, shamsiMonth);
        return new MisSyncRange
        {
            SyncFrom = start,
            SyncToExclusive = endExclusive,
            ShamsiYear = shamsiYear,
            ShamsiMonth = shamsiMonth,
            Description = $"ماه {shamsiYear}/{shamsiMonth:00}",
            IsBackfill = isBackfill
        };
    }
}

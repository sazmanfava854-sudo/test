using HRPerformance.Data;
using HRPerformance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Services.ExternalHr;

public class MisSyncStateService
{
    private readonly ApplicationDbContext _context;
    private readonly HrIntegrationSettingsService _settingsService;
    private readonly ILogger<MisSyncStateService> _logger;

    public MisSyncStateService(
        ApplicationDbContext context,
        HrIntegrationSettingsService settingsService,
        ILogger<MisSyncStateService> logger)
    {
        _context = context;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<HrMisSyncState> GetOrCreateStateAsync(Guid organizationId, CancellationToken ct = default)
    {
        var state = await _context.HrMisSyncStates
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, ct);

        if (state != null)
            return state;

        var settings = await _settingsService.GetRuntimeSettingsAsync(organizationId, ct);
        var targetYear = int.Parse(settings.ShamsiYearPrefix);
        var monthsBack = Math.Max(1, settings.InitialSyncMonthsBack);
        var (currentYear, currentMonth) = ShamsiDateHelper.GetCurrentShamsi();

        var startMonth = currentYear == targetYear ? currentMonth : 12;
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
        var settings = await _settingsService.GetRuntimeSettingsAsync(organizationId, ct);
        var mode = settings.SyncMode.Trim();

        if (mode.Equals("DaysBack", StringComparison.OrdinalIgnoreCase))
            return [CreateDaysBackRange(settings)];

        if (mode.Equals("DateRange", StringComparison.OrdinalIgnoreCase)
            && settings.SyncFromDate.HasValue
            && settings.SyncToDate.HasValue)
        {
            return
            [
                new MisSyncRange
                {
                    SyncFrom = settings.SyncFromDate.Value.Date,
                    SyncToExclusive = settings.SyncToDate.Value.Date.AddDays(1),
                    Description = $"بازه {settings.SyncFromDate:yyyy-MM-dd} تا {settings.SyncToDate:yyyy-MM-dd}"
                }
            ];
        }

        return await GetMonthlyRangesAsync(organizationId, settings, ct);
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

    private async Task<IReadOnlyList<MisSyncRange>> GetMonthlyRangesAsync(
        Guid organizationId,
        HrIntegrationRuntimeSettings settings,
        CancellationToken ct)
    {
        var state = await GetOrCreateStateAsync(organizationId, ct);
        var monthsPerRun = Math.Max(1, settings.MonthsPerSyncRun);
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

    private static MisSyncRange CreateDaysBackRange(HrIntegrationRuntimeSettings settings)
    {
        var syncFrom = DateTime.Today.AddDays(-settings.SyncDaysBack);
        return new MisSyncRange
        {
            SyncFrom = syncFrom,
            SyncToExclusive = DateTime.Today.AddDays(1),
            Description = $"{settings.SyncDaysBack} روز اخیر"
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

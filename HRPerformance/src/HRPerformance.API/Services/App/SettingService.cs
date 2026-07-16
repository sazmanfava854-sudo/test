using HRPerformance.Common;
using HRPerformance.DTOs.Settings;
using HRPerformance.Entities;
using HRPerformance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.App;

public class SettingService
{
    private readonly IUnitOfWork _uow;

    public SettingService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IList<SettingDto>>> GetAllAsync(Guid? organizationId, string? category, CancellationToken ct = default)
    {
        var query = _uow.Repository<Setting>().Query().Where(s => s.OrganizationId == organizationId);
        if (!string.IsNullOrEmpty(category)) query = query.Where(s => s.Category == category);
        var items = await query.ToListAsync(ct);
        return ApiResponse<IList<SettingDto>>.Ok(items.Select(s =>
            new SettingDto(s.Id, s.Key, s.Value, s.Category, s.Description, s.DataType)).ToList());
    }

    public async Task<ApiResponse<SettingDto>> UpdateAsync(Guid? organizationId, UpdateSettingRequest request, CancellationToken ct = default)
    {
        var setting = await _uow.Repository<Setting>().Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.Key == request.Key, ct);
        if (setting == null)
        {
            setting = new Setting { OrganizationId = organizationId, Key = request.Key, Value = request.Value, Category = "General" };
            await _uow.Repository<Setting>().AddAsync(setting, ct);
        }
        else
        {
            setting.Value = request.Value;
            setting.UpdatedAt = DateTime.UtcNow;
            await _uow.Repository<Setting>().UpdateAsync(setting, ct);
        }
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<SettingDto>.Ok(new SettingDto(setting.Id, setting.Key, setting.Value, setting.Category, setting.Description, setting.DataType));
    }

    public async Task<ApiResponse<IList<HolidayDto>>> GetHolidaysAsync(Guid organizationId, CancellationToken ct = default)
    {
        var items = await _uow.Repository<Holiday>().Query()
            .Where(h => h.OrganizationId == organizationId).OrderBy(h => h.HolidayDate).ToListAsync(ct);
        return ApiResponse<IList<HolidayDto>>.Ok(items.Select(h =>
            new HolidayDto(h.Id, h.Title, h.HolidayDate, h.IsRecurring, h.Description)).ToList());
    }

    public async Task<ApiResponse<HolidayDto>> CreateHolidayAsync(Guid organizationId, string title, DateTime holidayDate, bool isRecurring, CancellationToken ct = default)
    {
        var h = new Holiday { OrganizationId = organizationId, Title = title, HolidayDate = holidayDate, IsRecurring = isRecurring };
        await _uow.Repository<Holiday>().AddAsync(h, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<HolidayDto>.Ok(new HolidayDto(h.Id, h.Title, h.HolidayDate, h.IsRecurring, h.Description));
    }
}

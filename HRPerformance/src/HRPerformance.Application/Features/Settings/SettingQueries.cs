using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Settings;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Settings;
public record GetSettingsQuery(Guid? OrganizationId, string? Category) : IRequest<ApiResponse<IList<SettingDto>>>;
public record UpdateSettingCommand(Guid? OrganizationId, UpdateSettingRequest Request) : IRequest<ApiResponse<SettingDto>>;
public record GetHolidaysQuery(Guid OrganizationId) : IRequest<ApiResponse<IList<HolidayDto>>>;
public record CreateHolidayCommand(Guid OrganizationId, string Title, DateTime HolidayDate, bool IsRecurring) : IRequest<ApiResponse<HolidayDto>>;

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, ApiResponse<IList<SettingDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetSettingsQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<IList<SettingDto>>> Handle(GetSettingsQuery q, CancellationToken ct)
    {
        var query = _uow.Repository<Setting>().Query().Where(s => s.OrganizationId == q.OrganizationId);
        if (!string.IsNullOrEmpty(q.Category)) query = query.Where(s => s.Category == q.Category);
        var items = await query.ToListAsync(ct);
        return ApiResponse<IList<SettingDto>>.Ok(items.Select(s => new SettingDto(s.Id, s.Key, s.Value, s.Category, s.Description, s.DataType)).ToList());
    }
}

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, ApiResponse<SettingDto>>
{
    private readonly IUnitOfWork _uow;
    public UpdateSettingCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<SettingDto>> Handle(UpdateSettingCommand cmd, CancellationToken ct)
    {
        var setting = await _uow.Repository<Setting>().Query().FirstOrDefaultAsync(s => s.OrganizationId == cmd.OrganizationId && s.Key == cmd.Request.Key, ct);
        if (setting == null) { setting = new Setting { OrganizationId = cmd.OrganizationId, Key = cmd.Request.Key, Value = cmd.Request.Value, Category = "General" }; await _uow.Repository<Setting>().AddAsync(setting, ct); }
        else { setting.Value = cmd.Request.Value; setting.UpdatedAt = DateTime.UtcNow; await _uow.Repository<Setting>().UpdateAsync(setting, ct); }
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<SettingDto>.Ok(new SettingDto(setting.Id, setting.Key, setting.Value, setting.Category, setting.Description, setting.DataType));
    }
}

public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, ApiResponse<IList<HolidayDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetHolidaysQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<IList<HolidayDto>>> Handle(GetHolidaysQuery q, CancellationToken ct)
    {
        var items = await _uow.Repository<Holiday>().Query().Where(h => h.OrganizationId == q.OrganizationId).OrderBy(h => h.HolidayDate).ToListAsync(ct);
        return ApiResponse<IList<HolidayDto>>.Ok(items.Select(h => new HolidayDto(h.Id, h.Title, h.HolidayDate, h.IsRecurring, h.Description)).ToList());
    }
}

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, ApiResponse<HolidayDto>>
{
    private readonly IUnitOfWork _uow;
    public CreateHolidayCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<HolidayDto>> Handle(CreateHolidayCommand cmd, CancellationToken ct)
    {
        var h = new Holiday { OrganizationId = cmd.OrganizationId, Title = cmd.Title, HolidayDate = cmd.HolidayDate, IsRecurring = cmd.IsRecurring };
        await _uow.Repository<Holiday>().AddAsync(h, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<HolidayDto>.Ok(new HolidayDto(h.Id, h.Title, h.HolidayDate, h.IsRecurring, h.Description));
    }
}

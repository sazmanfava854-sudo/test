using HRPerformance.Common;
using HRPerformance.Entities;
using HRPerformance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.App;

public record NotificationDto(Guid Id, string Title, string Message, int Type, bool IsRead, DateTime CreatedAt);

public class NotificationAppService
{
    private readonly IUnitOfWork _uow;

    public NotificationAppService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IList<NotificationDto>>> GetAllAsync(Guid userId, bool? unreadOnly, CancellationToken ct = default)
    {
        var query = _uow.Repository<Notification>().Query().Where(n => n.UserId == userId);
        if (unreadOnly == true) query = query.Where(n => !n.IsRead);
        var items = await query.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(ct);
        return ApiResponse<IList<NotificationDto>>.Ok(items.Select(n =>
            new NotificationDto(n.Id, n.Title, n.Message, (int)n.Type, n.IsRead, n.CreatedAt)).ToList());
    }

    public async Task<ApiResponse<bool>> MarkReadAsync(Guid id, CancellationToken ct = default)
    {
        var n = await _uow.Repository<Notification>().GetByIdAsync(id, ct);
        if (n == null) return ApiResponse<bool>.Fail("اعلان یافت نشد");
        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await _uow.Repository<Notification>().UpdateAsync(n, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true);
    }
}

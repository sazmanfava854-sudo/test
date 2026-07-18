using HRPerformance.Application.Common;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Notifications;
public record NotificationDto(Guid Id, string Title, string Message, int Type, bool IsRead, DateTime CreatedAt);
public record GetNotificationsQuery(Guid UserId, bool? UnreadOnly) : IRequest<ApiResponse<IList<NotificationDto>>>;
public record MarkNotificationReadCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, ApiResponse<IList<NotificationDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetNotificationsQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<IList<NotificationDto>>> Handle(GetNotificationsQuery q, CancellationToken ct)
    {
        var query = _uow.Repository<Notification>().Query().Where(n => n.UserId == q.UserId);
        if (q.UnreadOnly == true) query = query.Where(n => !n.IsRead);
        var items = await query.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(ct);
        return ApiResponse<IList<NotificationDto>>.Ok(items.Select(n => new NotificationDto(n.Id, n.Title, n.Message, (int)n.Type, n.IsRead, n.CreatedAt)).ToList());
    }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _uow;
    public MarkNotificationReadCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<bool>> Handle(MarkNotificationReadCommand cmd, CancellationToken ct)
    {
        var n = await _uow.Repository<Notification>().GetByIdAsync(cmd.Id, ct);
        if (n == null) return ApiResponse<bool>.Fail("اعلان یافت نشد");
        n.IsRead = true; n.ReadAt = DateTime.UtcNow;
        await _uow.Repository<Notification>().UpdateAsync(n, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true);
    }
}

using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
namespace HRPerformance.Domain.Interfaces;
public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, NotificationType type, CancellationToken ct = default); Task SendToRoleAsync(string role, Guid organizationId, string title, string message, NotificationType type, CancellationToken ct = default);
}

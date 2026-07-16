using HRPerformance.Entities;
using HRPerformance.Enums;
namespace HRPerformance.Interfaces;
public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, NotificationType type, CancellationToken ct = default); Task SendToRoleAsync(string role, Guid organizationId, string title, string message, NotificationType type, CancellationToken ct = default);
}

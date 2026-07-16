using HRPerformance.Entities;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using HRPerformance.Data;
using HRPerformance.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services;
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly UserManager<ApplicationUser> _userManager;
    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hub, UserManager<ApplicationUser> userManager)
    { _context = context; _hub = hub; _userManager = userManager; }

    public async Task SendAsync(Guid userId, string title, string message, NotificationType type, CancellationToken ct = default)
    {
        var n = new Notification { UserId = userId, Title = title, Message = message, Type = type };
        _context.Notifications.Add(n);
        await _context.SaveChangesAsync(ct);
        await _hub.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new { n.Id, n.Title, n.Message, Type = (int)n.Type, n.CreatedAt }, ct);
    }

    public async Task SendToRoleAsync(string role, Guid organizationId, string title, string message, NotificationType type, CancellationToken ct = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        foreach (var user in users.Where(u => u.OrganizationId == organizationId))
            await SendAsync(user.Id, title, message, type, ct);
    }
}

// Services/Notifications/NotificationService.cs
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly INotificationTemplateService _templates;

        public NotificationService(Tutor_ManagerDatabaseContext context, INotificationTemplateService templates)
        {
            _context = context;
            _templates = templates;
        }

        public async Task SendAsync(int userId, NotificationType type, Dictionary<string, string> data)
        {
            var (title, message) = _templates.Build(type, data);

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message
            };

            _context.Set<Notification>().Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetForUserAsync(int userId, bool unreadOnly = false)
        {
            var query = _context.Set<Notification>().Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query.OrderByDescending(n => n.DateCreated).ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Set<Notification>().FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
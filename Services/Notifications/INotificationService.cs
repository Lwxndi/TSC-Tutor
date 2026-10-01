
using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(int userId, NotificationType type, Dictionary<string, string> data);
        Task NotifyAdminsAsync(NotificationType type, Dictionary<string, string> data);
        Task<List<Notification>> GetForUserAsync(int userId, bool unreadOnly = false);
        Task MarkAsReadAsync(int notificationId);
    }
}
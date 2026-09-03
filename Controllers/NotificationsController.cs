
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tutor_Manager.Services.Notifications;

namespace Tutor_Manager.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications)
        {
            _notifications = notifications;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Notifications/Dropdown
        // Returns just the HTML fragment for the bell dropdown, so it can be
        // reloaded/refreshed without a full page reload.
        [HttpGet]
        public async Task<IActionResult> Dropdown()
        {
            var notifications = await _notifications.GetForUserAsync(CurrentUserId);
            return PartialView("_NotificationDropdown", notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notifications.MarkAsReadAsync(id);
            return Ok();
        }
    }
}
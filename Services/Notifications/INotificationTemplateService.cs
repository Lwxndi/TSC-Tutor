
namespace Tutor_Manager.Services.Notifications
{
    public interface INotificationTemplateService
    {
        (string Title, string Message) Build(NotificationType type, Dictionary<string, string> data);
    }
}
// Services/Notifications/NotificationTemplateService.cs
namespace Tutor_Manager.Services.Notifications
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        public (string Title, string Message) Build(NotificationType type, Dictionary<string, string> data)
        {
            return type switch
            {
                NotificationType.RegistrationWelcome =>
                    ("Welcome!", $"Your TSC number is {data["TscNumber"]}. Welcome aboard, {data["FirstName"]}!"),

                NotificationType.GuardianLinked =>
                    ("Guardian Linked", $"{data["GuardianName"]} has been linked to your profile."),

                NotificationType.SessionBooked =>
                    ("Session Confirmed", $"Your {data["Subject"]} session on {data["Date"]} at {data["Location"]} is confirmed."),

                NotificationType.SessionReminder =>
                    ("Upcoming Session", $"Reminder: your {data["Subject"]} session is coming up on {data["Date"]}."),

                NotificationType.SessionCancelled =>
                    ("Session Cancelled", $"Your {data["Subject"]} session on {data["Date"]} was cancelled."),

                NotificationType.SessionMissed =>
                    ("Missed Session", $"You missed your {data["Subject"]} session on {data["Date"]}."),

                NotificationType.PaymentReceived =>
                    ("Payment Received", $"We've received your payment of R{data["Amount"]}."),

                NotificationType.GuardianUnlinked =>
                    ("Linking Needed", $"We couldn't find a learner with TSC number {data["TscNumber"]}. Try again from your dashboard."),

                NotificationType.PaymentFailed =>
                    ("Payment Failed", $"Your payment of R{data["Amount"]} could not be processed."),

                _ => throw new NotImplementedException($"No notification template defined for {type}")
            };
        }
    }
}
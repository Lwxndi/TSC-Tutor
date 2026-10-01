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

                NotificationType.SubjectDeactivated =>
                    ("Subject Deactivated", $"{data["SubjectName"]} has been deactivated and is no longer available for new offerings."),

                NotificationType.TutorDeactivated =>
                    ("Account Deactivated", "Your tutor account has been deactivated. Contact TSC administration for details."),

                NotificationType.TutorReactivated =>
                    ("Account Reactivated", "Your tutor account has been reactivated. Welcome back!"),
                NotificationType.OfferingAssigned =>
                    ("New Offering Assigned", $"You've been assigned to teach {data["Subject"]} (Grade {data["Grade"]})."),

                NotificationType.OfferingDeactivated =>
                    ("Offering Discontinued", $"{data["Subject"]} (Grade {data["Grade"]}) has been discontinued."),

                NotificationType.SessionCompleted =>
            ("Session Completed", $"{data["TutorName"]}'s {data["Subject"]} session (Grade {data["Grade"]}) on {data["Date"]} was marked completed."),

                NotificationType.PaymentReceivedAdmin =>
        ("Payment Received", $"{data["GuardianName"]} paid R{data["Amount"]} for {data["Learners"]}."),


                _ => throw new NotImplementedException($"No notification template defined for {type}")
            };
        }
    }
}
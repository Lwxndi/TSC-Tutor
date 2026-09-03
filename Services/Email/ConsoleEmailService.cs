using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Email
{
    public class ConsoleEmailService : IEmailService
    {
        public Task SendAsync(EmailMessage message)
        {
            Console.WriteLine($"--- EMAIL ---\nTo: {message.ToEmail}\nSubject: {message.Subject}\n\n{message.Body}\n-------------");
            return Task.CompletedTask;
        }
    }
}

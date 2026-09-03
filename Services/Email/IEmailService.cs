using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Email
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message);
    }
}

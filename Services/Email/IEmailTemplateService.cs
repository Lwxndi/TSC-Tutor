using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Email
{
    public interface IEmailTemplateService
    {
        EmailMessage Build(EmailType type, string toEmail, Dictionary<string, string> data);
    }
}

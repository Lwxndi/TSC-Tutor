using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(EmailMessage message)
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };
            mail.To.Add(message.ToEmail);

            await client.SendMailAsync(mail);
        }
    }
}
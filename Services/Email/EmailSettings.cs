namespace Tutor_Manager.Services.Email
{
    public class EmailSettings
    {
        public required string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }
        public required string AppPassword { get; set; }
    }
}

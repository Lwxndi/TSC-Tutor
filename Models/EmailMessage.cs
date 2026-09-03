namespace Tutor_Manager.Models
{
    public class EmailMessage
    {
        public required string ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool IsHtml { get; set; } = true;
    }
}

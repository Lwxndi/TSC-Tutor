namespace Tutor_Manager.Models
{
    public class RecurringBillingResult
    {
        public int Generated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
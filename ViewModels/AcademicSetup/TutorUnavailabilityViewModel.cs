namespace Tutor_Manager.ViewModels
{
    public class TutorUnavailabilityViewModel
    {
        public int TutorUserId { get; set; }
        public string TutorName { get; set; } = null!;
        public List<UnavailabilityEntryViewModel> Entries { get; set; } = new();
    }

    public class UnavailabilityEntryViewModel
    {
        public int TutorUnavailabilityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
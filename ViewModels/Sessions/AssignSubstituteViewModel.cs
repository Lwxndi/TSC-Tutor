namespace Tutor_Manager.ViewModels.Sessions
{
    public class AssignSubstituteViewModel
    {
        public int SessionId { get; set; }
        public SessionDetailViewModel SessionSummary { get; set; } = null!;
        public List<TutorDropdownItem> AvailableSubstitutes { get; set; } = new();
    }
}
namespace Tutor_Manager.ViewModels.Sessions
{
    public class LearnerTimetableViewModel
    {
        public int LearnerUserId { get; set; }
        public string LearnerName { get; set; } = null!;
        public List<SessionListItemViewModel> Sessions { get; set; } = new();
    }
}
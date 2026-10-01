namespace Tutor_Manager.ViewModels.Sessions
{
    public class TutorSessionBoardViewModel
    {
        public List<SessionListItemViewModel> Today { get; set; } = new();
        public List<SessionListItemViewModel> Upcoming { get; set; } = new();
        public List<SessionListItemViewModel> NeedsWrapUp { get; set; } = new();
    }
}
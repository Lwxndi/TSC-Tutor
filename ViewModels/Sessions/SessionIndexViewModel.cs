using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.ViewModels
{
    public class SessionIndexViewModel
    {
        public List<SessionListItemViewModel> Sessions { get; set; } = new();
        public List<SubjectDropdownItem> Subjects { get; set; } = new();
        public List<TutorDropdownItem> Tutors { get; set; } = new();
        public SessionFilterViewModel Filter { get; set; } = new();
    }
}
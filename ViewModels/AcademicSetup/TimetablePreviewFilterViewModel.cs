using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels.Sessions
{
    public class TimetablePreviewFilterViewModel
    {
        public int? SubjectId { get; set; }
        public Grade? Grade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SubjectDropdownItem> AvailableSubjects { get; set; } = new();
        public GenerationPreviewViewModel? Preview { get; set; }
    }
}
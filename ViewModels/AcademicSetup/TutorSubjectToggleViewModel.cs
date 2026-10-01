using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    // Posted when admin checks/unchecks a specific subject+grade box
    public class TutorSubjectToggleViewModel
    {
        public int TutorUserId { get; set; }
        public int SubjectId { get; set; }
        public Grade Grade { get; set; }
        public bool Assign { get; set; } // true = assign, false = remove
    }
}
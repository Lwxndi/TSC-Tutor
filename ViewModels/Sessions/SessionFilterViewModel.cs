using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class SessionFilterViewModel
    {
        public int? SubjectId { get; set; }
        public Grade? Grade { get; set; }
        public int? TutorUserId { get; set; }
        public string DateRange { get; set; } = "Today"; // Today, ThisWeek, ThisMonth, All
        public SessionStatus? Status { get; set; }
    }
}
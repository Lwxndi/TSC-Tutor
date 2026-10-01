using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.Sessions
{
    public class SessionListItemViewModel
    {
        public int SessionId { get; set; }
        public int OfferingId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SessionStatus Status { get; set; }
        public string? JitsiLink { get; set; }
        public Tutor_Manager.Models.Enums.DeliveryMethod DeliveryMethod { get; set; }
        public string? Venue { get; set; }
        public int UnmarkedCount { get; set; }
    }
}
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class SessionDetailViewModel
    {
        public int SessionId { get; set; }
        public int OfferingId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public int TutorUserId { get; set; }
        public bool IsSubstitute { get; set; }   // true when OverrideTutorUserId is set on the Session
        public DeliveryMethod DeliveryMethod { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SessionStatus Status { get; set; }
        public string? JitsiLink { get; set; }
    }
}
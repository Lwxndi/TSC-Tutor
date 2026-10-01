using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.Sessions
{
    public class AttendanceRowViewModel
    {
        public int LearnerUserId { get; set; }
        public string LearnerName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public int? MinutesLate { get; set; }
        public AttendedVia? AttendedVia { get; set; }
        public string? Note { get; set; }
        public bool IsMarked { get; set; }
        public bool Locked { get; set; }   // display only; the server re-checks on save
    }

    public class MarkAttendanceViewModel
    {
        public int SessionId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int GradeNumber { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SessionStatus Status { get; set; }
        public bool IsHybrid { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsAdminEdit { get; set; }
        public bool CanEdit { get; set; }
        public List<AttendanceRowViewModel> Rows { get; set; } = new();
    }
}
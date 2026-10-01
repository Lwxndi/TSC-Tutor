using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class Session
    {
        public int SessionId { get; set; }

        [ForeignKey("Offering")]
        public int OfferingId { get; set; }
        public Offering Offering { get; set; } = null!;

        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

        public string? JitsiLink { get; set; }   // Remote + Hybrid
        public string? Venue { get; set; }       // Physical + Hybrid

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

        [ForeignKey("CancelledBy")]
        public int? CancelledByUserId { get; set; }
        public User? CancelledBy { get; set; }

        public DateTime? RescheduledFromDate { get; set; }
        public TimeSpan? RescheduledFromStartTime { get; set; }
        public TimeSpan? RescheduledFromEndTime { get; set; }
        public string? RescheduleReason { get; set; }

        [ForeignKey("OverrideTutor")]
        public int? OverrideTutorUserId { get; set; }
        public Tutor? OverrideTutor { get; set; }

        // Old columns: delete in the S3 migration, once the form no longer uses them
        public string? TutorNotes { get; set; }
        public string? TopicsCovered { get; set; }
        public StudentPerformance? Performance { get; set; }
        public string? HomeworkAssigned { get; set; }
        public string? TutorComments { get; set; }

        public ICollection<SessionAttendance> Attendance { get; set; } = new List<SessionAttendance>();
        public ICollection<SessionNote> Notes { get; set; } = new List<SessionNote>();
        public ICollection<SessionTopic> Topics { get; set; } = new List<SessionTopic>();
        public ICollection<SessionLearnerRemark> LearnerRemarks { get; set; } = new List<SessionLearnerRemark>();
        public ICollection<SessionActivity> Activities { get; set; } = new List<SessionActivity>();
        public ICollection<SessionMaterial> Materials { get; set; } = new List<SessionMaterial>();
        public ICollection<SessionFeedback> Feedback { get; set; } = new List<SessionFeedback>();
    }
}
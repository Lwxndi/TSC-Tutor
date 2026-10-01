using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class SessionAttendance
    {
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public int? MinutesLate { get; set; }
        public AttendedVia? AttendedVia { get; set; }
        public string? Note { get; set; }

        public bool IsMarked { get; set; }
        public DateTime? MarkedAt { get; set; }

        [ForeignKey("MarkedBy")]
        public int? MarkedByUserId { get; set; }
        public User? MarkedBy { get; set; }

        [NotMapped]
        public bool IsLate => Status == AttendanceStatus.Present && (MinutesLate ?? 0) > 0;
    }

    public class SessionNote
    {
        public int SessionNoteId { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        [ForeignKey("Author")]
        public int AuthorUserId { get; set; }
        public User Author { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public bool IsShareable { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SessionTopic
    {
        public int SessionTopicId { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;   // trim + lowercase, set in the service
    }

    public class SessionLearnerRemark
    {
        public int SessionLearnerRemarkId { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        [ForeignKey("Author")]
        public int AuthorUserId { get; set; }
        public User Author { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public bool IsShareable { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SessionActivity
    {
        public int SessionActivityId { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ActivityType Type { get; set; }
        public DateTime? DueDate { get; set; }
        public string? TopicName { get; set; }

        public int? QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        public int? AssessmentId { get; set; }
        public Assessment? Assessment { get; set; }

        public ICollection<SessionActivityMaterial> Materials { get; set; } = new List<SessionActivityMaterial>();
        public ICollection<SessionActivityResult> Results { get; set; } = new List<SessionActivityResult>();
    }

    public class SessionMaterial
    {
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;
        public int StudyMaterialId { get; set; }
        public StudyMaterial StudyMaterial { get; set; } = null!;
    }

    public class SessionActivityMaterial
    {
        public int SessionActivityId { get; set; }
        public SessionActivity SessionActivity { get; set; } = null!;
        public int StudyMaterialId { get; set; }
        public StudyMaterial StudyMaterial { get; set; } = null!;
    }

    public class SessionActivityResult
    {
        public int SessionActivityId { get; set; }
        public SessionActivity SessionActivity { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        public decimal? Score { get; set; }
        public decimal? MaxScore { get; set; }
        public string? Comment { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RecordedBy")]
        public int RecordedByUserId { get; set; }
        public User RecordedBy { get; set; } = null!;
    }

    public class SessionFeedback
    {
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        public int Rating { get; set; }          // 1 to 5, validated in the service
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
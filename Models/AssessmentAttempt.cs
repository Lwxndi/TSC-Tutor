using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class AssessmentAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; } = null!;

        [Required]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        [Required]
        public AssessmentAttemptStatus Status { get; set; } = AssessmentAttemptStatus.InProgress;

        public bool IsAutoSubmitted { get; set; }

        public int? TotalMarksAwarded { get; set; }
        public int? TotalPossibleMarks { get; set; }

        public ICollection<AssessmentAttemptAnswer> Answers { get; set; } = new List<AssessmentAttemptAnswer>();
    }

    public enum AssessmentAttemptStatus
    {
        InProgress,
        Submitted,
        Abandoned
    }
}
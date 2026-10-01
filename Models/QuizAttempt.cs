using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        [Required]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        [Required]
        public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;

        public bool IsAutoSubmitted { get; set; }

        public int? TotalMarksAwarded { get; set; }

        public int? TotalPossibleMarks { get; set; }

        public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    }

    public enum QuizAttemptStatus
    {
        InProgress,
        Submitted,
        Abandoned
    }
}
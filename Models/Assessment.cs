using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class Assessment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        [Required]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Informational only — display/sort field. NOT used as a lock; see OpenAt.
        [Required]
        public DateTime DueDate { get; set; }

        public int? DeclaredTotalMarks { get; set; }

        [Required]
        public AssessmentGenerationStatus Status { get; set; } = AssessmentGenerationStatus.NotGenerated;

        public DateTime? GeneratedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // --- Added in Step 10: constraints, shared shape with Quiz ---
        [Required]
        public DateTime OpenAt { get; set; }

        [Required]
        public DateTime CloseAt { get; set; }

        [Required]
        public int MaxAttempts { get; set; }

        [Required]
        public QuizzTypes AudienceType { get; set; }

        public int? OfferingId { get; set; }
        public Offering? Offering { get; set; }

        public ICollection<AssessmentLearner> AudienceLearners { get; set; } = new List<AssessmentLearner>();

        public ICollection<AssessmentFile> Files { get; set; } = new List<AssessmentFile>();
        public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
    }

    public enum AssessmentGenerationStatus
    {
        NotGenerated,
        Generated,
        Confirmed
    }
}
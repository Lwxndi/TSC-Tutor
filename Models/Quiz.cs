// Models/Quiz.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudyMaterialId { get; set; }
        public StudyMaterial StudyMaterial { get; set; } = null!;

        [Required]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        // --- Constraints ---
        [Required]
        public DateTime OpenAt { get; set; }

        [Required]
        public DateTime CloseAt { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public int MaxAttempts { get; set; }

        [Required]
        public QuizzTypes AudienceType { get; set; }

        // Only set when AudienceType == SpecificOffering
        public int? OfferingId { get; set; }
        public Offering? Offering { get; set; }

        // Only populated when AudienceType == SpecificLearners
        public ICollection<LearnerQuizz> AudienceLearners { get; set; } = new List<LearnerQuizz>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Add these members inside the existing Quiz class:

        [Required]
        public QuizDifficultyLevel DifficultyLevel { get; set; }

        [Required]
        [Range(0, 50)]
        public int MultipleChoiceSingleCount { get; set; }

        [Required]
        [Range(0, 50)]
        public int MultipleChoiceMultipleCount { get; set; }

        [Required]
        [Range(0, 50)]
        public int TrueFalseCount { get; set; }

        [Required]
        [Range(0, 50)]
        public int ShortAnswerCount { get; set; }

        [Required]
        [Range(0, 50)]
        public int EssayCount { get; set; }

        [Required]
        [Range(0, 50)]
        public int NumericEquationCount { get; set; }

        [Required]
        public QuizGenerationStatus Status { get; set; } = QuizGenerationStatus.NotGenerated;

        public DateTime? GeneratedAt { get; set; }

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

    }

    public enum QuizzTypes
    {
        AllEnrolledInSubject,
        SpecificOffering,
        SpecificLearners
    }
}
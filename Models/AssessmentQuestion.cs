// Models/AssessmentQuestion.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class AssessmentQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; } = null!;

        // May contain LaTeX, same convention as QuizQuestion
        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        public int Marks { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        [Required]
        public string MarkingGuidance { get; set; } = null!;
    }
}
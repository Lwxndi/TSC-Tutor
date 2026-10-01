using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class QuizQuestionOption
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("QuizQuestion")]
        public int QuizQuestionId { get; set; }
        public QuizQuestion QuizQuestion { get; set; } = null!;

        // May contain LaTeX, same as QuestionText
        [Required]
        public string OptionText { get; set; } = null!;

        [Required]
        public bool IsCorrect { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
    }
}
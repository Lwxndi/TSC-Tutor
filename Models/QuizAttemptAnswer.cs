using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class QuizAttemptAnswer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("QuizAttempt")]
        public int QuizAttemptId { get; set; }
        public QuizAttempt QuizAttempt { get; set; } = null!;

        [ForeignKey("QuizQuestion")]
        public int QuizQuestionId { get; set; }
        public QuizQuestion QuizQuestion { get; set; } = null!;

        // Used for ShortAnswer, Essay, NumericEquation
        public string? TextAnswer { get; set; }

        // Used for TrueFalse
        public bool? BoolAnswer { get; set; }


        public string? QuestionTextSnapshot { get; set; }

        // Used for MultipleChoiceSingle / MultipleChoiceMultiple
        public ICollection<QuizAttemptSelectedOption> SelectedOptions { get; set; } = new List<QuizAttemptSelectedOption>();

        public int? MarksAwarded { get; set; }

        // AI-generated feedback, only populated for free-text types
        public string? AiFeedback { get; set; }
    }
}
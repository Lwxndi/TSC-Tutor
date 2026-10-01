using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        [Required]
        public QuizQuestionType QuestionType { get; set; }

        // May contain LaTeX, e.g. "Solve for $x$: $2x + 3 = 7$"
        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        public int Marks { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        // Used for ShortAnswer, Essay, NumericEquation — the AI-generated marking guide
        // for this specific question. Null for MultipleChoice*/TrueFalse, where correctness
        // is structural (Options.IsCorrect / CorrectBoolAnswer) rather than judged.
        public string? MarkingGuidance { get; set; }

        // Used only for TrueFalse
        public bool? CorrectBoolAnswer { get; set; }
        // QuizQuestion.cs
        public string? ImagePath { get; set; }   // file name inside App_Data/quiz-images
        public ICollection<QuizQuestionOption> Options { get; set; } = new List<QuizQuestionOption>();
    }

    public enum QuizQuestionType
    {
        MultipleChoiceSingle,
        MultipleChoiceMultiple,
        TrueFalse,
        ShortAnswer,
        Essay,
        NumericEquation
    }

    public enum QuizDifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum QuizGenerationStatus
    {
        NotGenerated,
        Generated,
        Confirmed
        // "Confirmed" (after tutor review) is Step 4 scope, not added yet.
    }
}
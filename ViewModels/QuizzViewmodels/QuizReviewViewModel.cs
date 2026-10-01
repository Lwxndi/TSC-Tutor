using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizReviewViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public QuizGenerationStatus Status { get; set; }
        public List<QuizQuestionSummaryViewModel> Questions { get; set; } = new();
    }

    public class QuizQuestionSummaryViewModel
    {
        public int Id { get; set; }
        public QuizQuestionType QuestionType { get; set; }
        public string QuestionText { get; set; } = null!;
        public int Marks { get; set; }
        public int DisplayOrder { get; set; }
        public bool HasImage { get; set; }
        public bool? CorrectBoolAnswer { get; set; }
        public string? MarkingGuidance { get; set; }
        public List<QuizOptionSummaryViewModel> Options { get; set; } = new();
    }

    public class QuizOptionSummaryViewModel
    {
        public string OptionText { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; }
    }
}
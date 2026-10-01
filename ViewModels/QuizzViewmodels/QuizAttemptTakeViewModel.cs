namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizAttemptTakeViewModel
    {
        public int AttemptId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public int DurationMinutes { get; set; }
        public List<QuizAttemptQuestionViewModel> Questions { get; set; } = new();
    }

    public class QuizAttemptQuestionViewModel
    {
        public int QuestionId { get; set; }
        public Models.QuizQuestionType QuestionType { get; set; }
        public string QuestionText { get; set; } = null!;
        public int Marks { get; set; }
        public string? ImageUrl { get; set; }
        public bool HasImage { get; set; }
        public List<QuizAttemptOptionViewModel> Options { get; set; } = new();
    }

    public class QuizAttemptOptionViewModel
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = null!;
        // Deliberately no IsCorrect here
    }
}
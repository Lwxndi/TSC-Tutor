namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizAttemptResultViewModel
    {
        public int AttemptId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public int? TotalMarksAwarded { get; set; }
        public int? TotalPossibleMarks { get; set; }
        public bool IsAutoSubmitted { get; set; }
        public List<QuizAttemptAnswerResultViewModel> Answers { get; set; } = new();
    }

    public class QuizAttemptAnswerResultViewModel
    {
        public string QuestionText { get; set; } = null!;
        public Models.QuizQuestionType QuestionType { get; set; }
        public int Marks { get; set; }
        public int? MarksAwarded { get; set; }
        public string? AiFeedback { get; set; }
    }
}
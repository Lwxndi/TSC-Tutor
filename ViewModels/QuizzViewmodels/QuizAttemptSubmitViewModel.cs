namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizAttemptSubmitViewModel
    {
        public int AttemptId { get; set; }
        public bool IsAutoSubmit { get; set; }
        public List<QuizAttemptAnswerSubmitViewModel> Answers { get; set; } = new();
    }

    public class QuizAttemptAnswerSubmitViewModel
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public bool? BoolAnswer { get; set; }
        public string? TextAnswer { get; set; }
    }
}
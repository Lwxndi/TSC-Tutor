namespace Tutor_Manager.ViewModels.AssessmentViewmodels
{
    public class AvailableAssessmentViewModel
    {
        public int AssessmentId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime OpenAt { get; set; }
        public DateTime CloseAt { get; set; }
        public int MaxAttempts { get; set; }
        public int SubmittedAttemptCount { get; set; }
        public bool HasInProgressAttempt { get; set; }
        public int? InProgressAttemptId { get; set; }
    }

    public class AssessmentAttemptTakeViewModel
    {
        public int AttemptId { get; set; }
        public string AssessmentTitle { get; set; } = null!;
        public List<AssessmentAttemptQuestionViewModel> Questions { get; set; } = new();
    }

    public class AssessmentAttemptQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int Marks { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class AssessmentAttemptSubmitViewModel
    {
        public int AttemptId { get; set; }
        public bool IsAutoSubmit { get; set; }
        public List<AssessmentAttemptAnswerSubmitViewModel> Answers { get; set; } = new();
    }

    public class AssessmentAttemptAnswerSubmitViewModel
    {
        public int QuestionId { get; set; }
        public string? TextAnswer { get; set; }
    }

    public class AssessmentAttemptResultViewModel
    {
        public int AttemptId { get; set; }
        public string AssessmentTitle { get; set; } = null!;
        public int? TotalMarksAwarded { get; set; }
        public int? TotalPossibleMarks { get; set; }
        public bool IsAutoSubmitted { get; set; }
        public List<AssessmentAttemptAnswerResultViewModel> Answers { get; set; } = new();
    }

    public class AssessmentAttemptAnswerResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int Marks { get; set; }
        public int? MarksAwarded { get; set; }
        public string? AiFeedback { get; set; }
    }
}
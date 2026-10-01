// ViewModels/QuizzViewmodels/QuizResultSummaryViewModel.cs
namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizResultSummaryViewModel
    {
        public int AttemptId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public int? TotalMarksAwarded { get; set; }
        public int? TotalPossibleMarks { get; set; }
        public bool IsAutoSubmitted { get; set; }
    }
}
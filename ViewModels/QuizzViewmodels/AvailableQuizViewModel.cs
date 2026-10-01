namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class AvailableQuizViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string StudyMaterialTitle { get; set; } = null!;
        public DateTime OpenAt { get; set; }
        public DateTime CloseAt { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxAttempts { get; set; }
        public int SubmittedAttemptCount { get; set; }
        public bool HasInProgressAttempt { get; set; }
        public int? InProgressAttemptId { get; set; }
    }
}
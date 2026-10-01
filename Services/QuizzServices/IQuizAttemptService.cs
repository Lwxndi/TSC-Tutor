using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public interface IQuizAttemptService
    {
        Task<List<AvailableQuizViewModel>> GetAvailableQuizzesAsync(int learnerUserId);
        Task<StudyMaterialResult> StartOrResumeAttemptAsync(int quizId, int learnerUserId);
        Task<QuizAttemptTakeViewModel?> GetAttemptForTakingAsync(int attemptId, int learnerUserId);
        Task<StudyMaterialResult> SubmitAttemptAsync(QuizAttemptSubmitViewModel model, int learnerUserId);
        Task<QuizAttemptResultViewModel?> GetResultAsync(int attemptId, int learnerUserId);
        Task<List<QuizResultSummaryViewModel>> GetResultsHistoryAsync(int learnerUserId);

    }
}
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public interface IAssessmentAttemptService
    {
        Task<List<AvailableAssessmentViewModel>> GetAvailableAssessmentsAsync(int learnerUserId);
        Task<StudyMaterialResult> StartOrResumeAttemptAsync(int assessmentId, int learnerUserId);
        Task<AssessmentAttemptTakeViewModel?> GetAttemptForTakingAsync(int attemptId, int learnerUserId);
        Task<StudyMaterialResult> SubmitAttemptAsync(AssessmentAttemptSubmitViewModel model, int learnerUserId);
        Task<AssessmentAttemptResultViewModel?> GetResultAsync(int attemptId, int learnerUserId);
    }
}
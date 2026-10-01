using Tutor_Manager.ViewModels.TutorAssignment;

namespace Tutor_Manager.Services
{
    public interface IAiRecommendationService
    {
        Task<AiRecommendationResult> GenerateRecommendationAsync(int tutorUserId);
        Task<List<AiRecommendationResult>> GenerateForTutorsAsync(List<int> tutorUserIds);
        Task<AiRecommendationResult?> GetLatestRecommendationAsync(int tutorUserId);
    }
}
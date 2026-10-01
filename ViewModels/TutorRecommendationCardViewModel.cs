using Tutor_Manager.ViewModels.TutorAssignment;

namespace Tutor_Manager.ViewModels
{
    public class TutorRecommendationCardViewModel
    {
        public int TutorUserId { get; set; }
        public string TutorName { get; set; } = "";
        public AiRecommendationResult? Recommendation { get; set; }
    }
}
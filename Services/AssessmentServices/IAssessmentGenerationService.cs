// Services/AssessmentServices/IAssessmentGenerationService.cs
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public interface IAssessmentGenerationService
    {
        Task<QuizGenerationResult> GenerateAsync(int assessmentId, int tutorUserId);
    }
}
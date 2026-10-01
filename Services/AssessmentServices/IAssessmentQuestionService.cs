// Services/AssessmentServices/IAssessmentQuestionService.cs
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public interface IAssessmentQuestionService
    {
        Task<Assessment?> GetForReviewAsync(int assessmentId, int tutorUserId);
        Task<AssessmentQuestion?> GetQuestionAsync(int questionId, int assessmentId, int tutorUserId);
        Task<StudyMaterialResult> UpdateQuestionAsync(AssessmentQuestionEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> AddQuestionAsync(AssessmentQuestionEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> DeleteQuestionAsync(int questionId, int assessmentId, int tutorUserId);
        Task<StudyMaterialResult> ConfirmAsync(int assessmentId, int tutorUserId);
        Task<StudyMaterialResult> UnconfirmAsync(int assessmentId, int tutorUserId);

    }
}
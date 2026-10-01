// Services/AssessmentServices/IAssessmentService.cs
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public interface IAssessmentService
    {
        Task<StudyMaterialResult> CreateAsync(AssessmentCreateViewModel model, int tutorUserId);
        Task<Assessment?> GetByIdAsync(int id, int tutorUserId);
        Task<StudyMaterialResult> UpdateAsync(AssessmentEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> DeleteAsync(int id, int tutorUserId);
        Task<StudyMaterialResult> BulkDeleteAsync(List<int> ids, int tutorUserId);
    }
}
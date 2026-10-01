// Services/IStudyMaterialService.cs
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;
namespace Tutor_Manager.Services
{
    public interface IStudyMaterialService
    {
        Task<StudyMaterialResult> CreateAsync( StudyMaterialUploadViewModel model, int tutorUserId);
       
        Task<StudyMaterial?> GetByIdAsync(int id, int tutorUserId);
        Task<StudyMaterialResult> UpdateAsync(StudyMaterialEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> DeleteAsync(int id, int tutorUserId);
        Task<StudyMaterialResult> BulkDeleteAsync(List<int> ids, int tutorUserId);
    }
}
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public interface IQuizService
    {
        Task<StudyMaterialResult> CreateAsync(QuizCreateViewModel model, int tutorUserId);
        Task<Quiz?> GetByIdAsync(int id, int tutorUserId);
        Task<StudyMaterialResult> UpdateAsync(QuizEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> DeleteAsync(int id, int tutorUserId);
        Task<StudyMaterialResult> BulkDeleteAsync(List<int> ids, int tutorUserId);
    }
}
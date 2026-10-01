using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public interface IQuizQuestionService
    {
        Task<Quiz?> GetQuizForReviewAsync(int quizId, int tutorUserId);
        Task<QuizQuestion?> GetQuestionAsync(int questionId, int quizId, int tutorUserId);
        Task<StudyMaterialResult> UpdateQuestionAsync(QuizQuestionEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> AddQuestionAsync(QuizQuestionEditViewModel model, int tutorUserId);
        Task<StudyMaterialResult> DeleteQuestionAsync(int questionId, int quizId, int tutorUserId);
        Task<StudyMaterialResult> ConfirmAsync(int quizId, int tutorUserId);
        Task<StudyMaterialResult> UnconfirmAsync(int quizId, int tutorUserId);
    }
}
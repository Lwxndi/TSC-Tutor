using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public interface IQuizGenerationService
    {
        Task<QuizGenerationResult> GenerateQuestionsAsync(int quizId, int tutorUserId);
    }
}
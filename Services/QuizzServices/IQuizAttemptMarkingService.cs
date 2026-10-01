using Tutor_Manager.Models;

namespace Tutor_Manager.Services.QuizzServices
{
    public interface IQuizAttemptMarkingService
    {
        Task<(int marksAwarded, string feedback)> MarkFreeTextAnswerAsync(QuizQuestion question, string studentAnswer);
    }
}
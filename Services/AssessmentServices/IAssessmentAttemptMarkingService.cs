// Services/AssessmentServices/IAssessmentAttemptMarkingService.cs
using Tutor_Manager.Models;

namespace Tutor_Manager.Services.AssessmentServices
{
    public interface IAssessmentAttemptMarkingService
    {
        Task<(int marksAwarded, string feedback)> MarkAnswerAsync(AssessmentQuestion question, string studentAnswer);
    }
}
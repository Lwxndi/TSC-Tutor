// Services/QuizzServices/QuizAttemptMarkingService.cs
using System.Text;
using System.Text.Json;
using Tutor_Manager.Models;
using Tutor_Manager.Services.AiProviders;

namespace Tutor_Manager.Services.QuizzServices
{
    public class QuizAttemptMarkingService : IQuizAttemptMarkingService
    {
        private readonly IAiCompletionOrchestrator _orchestrator;
        private readonly ILogger<QuizAttemptMarkingService> _logger;

        public QuizAttemptMarkingService(
            IAiCompletionOrchestrator orchestrator,
            ILogger<QuizAttemptMarkingService> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        public async Task<(int marksAwarded, string feedback)> MarkFreeTextAnswerAsync(QuizQuestion question, string studentAnswer)
        {
            if (string.IsNullOrWhiteSpace(studentAnswer))
                return (0, "No answer was submitted.");

            var prompt = BuildPrompt(question, studentAnswer);

            try
            {
                var responseText = await _orchestrator.CompleteAsync(
                    prompt,
                    new AiCompletionOptions(Temperature: 0.2, MaxOutputTokens: 1024));

                return ParseResponse(responseText, question.Marks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI marking failed for question {QuestionId}", question.Id);
                // Nothing-lost principle: the answer itself is already saved by the caller
                // before this runs, so a marking failure loses no student work.
                return (0, "Automatic marking failed for this answer. It may need manual review.");
            }
        }

        private static string BuildPrompt(QuizQuestion question, string studentAnswer)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are marking a single quiz answer. Be fair and consistent.");
            sb.AppendLine($"Question type: {question.QuestionType}");
            sb.AppendLine($"Question: {question.QuestionText}");
            sb.AppendLine($"Maximum marks: {question.Marks}");
            sb.AppendLine($"Marking guidance: {question.MarkingGuidance}");
            sb.AppendLine($"Student's answer: {studentAnswer}");
            sb.AppendLine();
            if (question.QuestionType == QuizQuestionType.NumericEquation)
            {
                sb.AppendLine("This is a numeric/equation answer. Judge mathematical or algebraic equivalence, not exact text match — e.g. 'x^2+2x+1' and '(x+1)^2' should be treated as equivalent if the guidance says so. Follow any partial-credit rules in the marking guidance (e.g. correct method with an arithmetic slip).");
            }
            sb.AppendLine("Respond ONLY with valid JSON in exactly this format, no markdown fences, no other text:");
            sb.AppendLine(@"{ ""marksAwarded"": 2, ""feedback"": ""short explanation of the mark given"" }");
            return sb.ToString();
        }

        private static (int, string) ParseResponse(string responseText, int maxMarks)
        {
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
                responseText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);

            using var doc = JsonDocument.Parse(responseText);
            var marks = doc.RootElement.GetProperty("marksAwarded").GetInt32();
            var feedback = doc.RootElement.TryGetProperty("feedback", out var fb) ? fb.GetString() ?? "" : "";

            // Clamp defensively in case the model ignores the max — never trust the AI's number blindly.
            marks = Math.Clamp(marks, 0, maxMarks);

            return (marks, feedback);
        }
    }
}
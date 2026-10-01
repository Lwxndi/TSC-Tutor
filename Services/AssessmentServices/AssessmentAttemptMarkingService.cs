// Services/AssessmentServices/AssessmentAttemptMarkingService.cs
using System.Text;
using System.Text.Json;
using Tutor_Manager.Models;
using Tutor_Manager.Services.AiProviders;

namespace Tutor_Manager.Services.AssessmentServices
{
    public class AssessmentAttemptMarkingService : IAssessmentAttemptMarkingService
    {
        private readonly IAiCompletionOrchestrator _orchestrator;
        private readonly ILogger<AssessmentAttemptMarkingService> _logger;

        public AssessmentAttemptMarkingService(
            IAiCompletionOrchestrator orchestrator,
            ILogger<AssessmentAttemptMarkingService> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        public async Task<(int marksAwarded, string feedback)> MarkAnswerAsync(AssessmentQuestion question, string studentAnswer)
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
                _logger.LogError(ex, "AI marking failed for assessment question {QuestionId}", question.Id);
                // Nothing-lost principle, same as before — the raw answer is already saved.
                return (0, "Automatic marking failed for this answer. It may need manual review.");
            }
        }

        private static string BuildPrompt(AssessmentQuestion question, string studentAnswer)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are marking a single answer from a formal assessment, using the tutor's own marking guideline. Be fair and consistent, and follow the guideline strictly rather than external knowledge.");
            sb.AppendLine($"Question: {question.QuestionText}");
            sb.AppendLine($"Maximum marks: {question.Marks}");
            sb.AppendLine($"Marking guideline: {question.MarkingGuidance}");
            sb.AppendLine($"Student's answer: {studentAnswer}");
            sb.AppendLine();
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

            marks = Math.Clamp(marks, 0, maxMarks);
            return (marks, feedback);
        }
    }
}
// Services/QuizzServices/QuizGenerationService.cs
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Services.AiProviders;
using Tutor_Manager.Services.StudyMaterialTextExtraction;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public class QuizGenerationService : IQuizGenerationService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAiCompletionOrchestrator _orchestrator;
        private readonly IStudyMaterialTextExtractionService _textExtraction;
        private readonly IStudyMaterialImageExtractionService _imageExtraction;
        private readonly IQuizImageStorage _imageStorage;
        private readonly ILogger<QuizGenerationService> _logger;

        public QuizGenerationService(
            Tutor_ManagerDatabaseContext context,
            IAiCompletionOrchestrator orchestrator,
            IStudyMaterialTextExtractionService textExtraction,
            IStudyMaterialImageExtractionService imageExtraction,
            IQuizImageStorage imageStorage,
            ILogger<QuizGenerationService> logger)
        {
            _context = context;
            _orchestrator = orchestrator;
            _textExtraction = textExtraction;
            _imageExtraction = imageExtraction;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        public async Task<QuizGenerationResult> GenerateQuestionsAsync(int quizId, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.StudyMaterial).ThenInclude(m => m.Files)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.TutorUserId == tutorUserId && q.IsActive);

            if (quiz == null)
                return QuizGenerationResult.Failure("Quiz not found.");

            if (quiz.Status != QuizGenerationStatus.NotGenerated)
                return QuizGenerationResult.Failure("Questions have already been generated for this quiz.");

            var groundingText = await _textExtraction.ExtractTextAsync(quiz.StudyMaterial);
            if (string.IsNullOrWhiteSpace(groundingText))
                return QuizGenerationResult.Failure("Could not extract any text from the selected study material's files.");

            var images = _imageExtraction.Extract(quiz.StudyMaterial);
            var prompt = BuildPrompt(quiz, groundingText, images.Count);

            string responseText;
            try
            {
                responseText = await _orchestrator.CompleteAsync(
                    prompt,
                    new AiCompletionOptions(
                        Temperature: 0.4,
                        MaxOutputTokens: 8192,
                        JsonOutput: true,
                        Images: images.Select(i => new AiImage(i.Ref, i.MimeType, i.Bytes)).ToList()));

                _logger.LogDebug("Raw AI response (Quiz {QuizId}): {ResponseText}", quizId, responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI quiz generation call failed for quiz {QuizId}", quizId);
                return QuizGenerationResult.Failure("The AI generation call failed. Please try again.");
            }

            List<QuizQuestion> questions;
            try
            {
                questions = ParseResponse(responseText, quiz.Id, images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse AI quiz generation response for quiz {QuizId}", quizId);
                return QuizGenerationResult.Failure("Could not parse the AI response. Please try again.");
            }

            if (!questions.Any())
                return QuizGenerationResult.Failure("The AI did not return any questions. Please try again.");

            _context.QuizQuestions.AddRange(questions);
            quiz.Status = QuizGenerationStatus.Generated;
            quiz.GeneratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return QuizGenerationResult.Success();
        }

        private static string BuildPrompt(Quiz quiz, string groundingText, int imageCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are generating a quiz for a tutoring platform, strictly grounded in the study material text below.");
            sb.AppendLine("Do NOT use any outside knowledge beyond what is in the study material text. If the material does not cover something needed, do your best within what it does cover.");
            sb.AppendLine();
            sb.AppendLine($"Difficulty level: {quiz.DifficultyLevel}");
            sb.AppendLine("Generate exactly this many questions of each type:");
            sb.AppendLine($"- MultipleChoiceSingle: {quiz.MultipleChoiceSingleCount} (exactly one correct option each)");
            sb.AppendLine($"- MultipleChoiceMultiple: {quiz.MultipleChoiceMultipleCount} (one or more correct options each)");
            sb.AppendLine($"- TrueFalse: {quiz.TrueFalseCount}");
            sb.AppendLine($"- ShortAnswer: {quiz.ShortAnswerCount} (free text, provide markingGuidance)");
            sb.AppendLine($"- Essay: {quiz.EssayCount} (free text, provide markingGuidance)");
            sb.AppendLine($"- NumericEquation: {quiz.NumericEquationCount} (equations/numeric answers, provide markingGuidance describing the correct answer and how to judge mathematical/algebraic equivalence — e.g. accept equivalent forms, and describe partial-credit rules for correct method with an arithmetic slip)");
            sb.AppendLine();
            sb.AppendLine("For any mathematical, chemical, or physics notation, formulas, or equations anywhere in questionText, optionText, or markingGuidance, use LaTeX syntax delimited with single $ for inline math and double $$ for block math (e.g. \"Solve for $x$: $2x + 3 = 7$\"). Do not use LaTeX for plain text.");
            sb.AppendLine();
            sb.AppendLine("Assign a sensible 'marks' value (integer) to every question based on its complexity.");
            sb.AppendLine();

            if (imageCount > 0)
            {
                sb.AppendLine($"The study material also contains {imageCount} images, provided above as Image 1 to Image {imageCount}. When a question is about a diagram, figure or picture, set \"imageRef\" to that image's number so it is shown with the question, and make the question answerable by looking at that image. Use \"imageRef\": null for questions that do not need an image. Never invent an imageRef.");
                sb.AppendLine();
            }

            sb.AppendLine("Study material text:");
            sb.AppendLine("-----");
            sb.AppendLine(groundingText);
            sb.AppendLine("-----");
            sb.AppendLine();
            sb.AppendLine("Respond ONLY with valid JSON in exactly this format, no markdown fences, no other text:");
            sb.AppendLine(@"{
  ""questions"": [
    {
      ""type"": ""MultipleChoiceSingle"",
      ""text"": ""question text here"",
      ""marks"": 2,
      ""imageRef"": null,
      ""options"": [ { ""text"": ""option text"", ""isCorrect"": true } ],
      ""correctBoolAnswer"": null,
      ""markingGuidance"": null
    },
    {
      ""type"": ""TrueFalse"",
      ""text"": ""statement here"",
      ""marks"": 1,
      ""imageRef"": null,
      ""options"": null,
      ""correctBoolAnswer"": true,
      ""markingGuidance"": null
    },
    {
      ""type"": ""ShortAnswer"",
      ""text"": ""question text here"",
      ""marks"": 4,
      ""imageRef"": null,
      ""options"": null,
      ""correctBoolAnswer"": null,
      ""markingGuidance"": ""guidance for marking here""
    }
  ]
}");
            return sb.ToString();
        }

        private List<QuizQuestion> ParseResponse(string responseText, int quizId, List<ExtractedImage> images)
        {
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
                responseText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);

            var result = new List<QuizQuestion>();
            using var doc = JsonDocument.Parse(responseText);
            var order = 1;

            foreach (var item in doc.RootElement.GetProperty("questions").EnumerateArray())
            {
                var typeString = item.GetProperty("type").GetString() ?? "";
                if (!Enum.TryParse<QuizQuestionType>(typeString, out var questionType))
                    continue; // unparseable type: drop this one question rather than fail the whole batch

                var question = new QuizQuestion
                {
                    QuizId = quizId,
                    QuestionType = questionType,
                    QuestionText = item.GetProperty("text").GetString() ?? "",
                    Marks = item.TryGetProperty("marks", out var marksEl) ? marksEl.GetInt32() : 1,
                    DisplayOrder = order++,
                    MarkingGuidance = item.TryGetProperty("markingGuidance", out var mgEl) && mgEl.ValueKind != JsonValueKind.Null
                        ? mgEl.GetString()
                        : null,
                    CorrectBoolAnswer = item.TryGetProperty("correctBoolAnswer", out var cbEl) && cbEl.ValueKind != JsonValueKind.Null
                        ? cbEl.GetBoolean()
                        : null
                };

                if (item.TryGetProperty("imageRef", out var irEl) && irEl.ValueKind == JsonValueKind.Number)
                {
                    var img = images.FirstOrDefault(i => i.Ref == irEl.GetInt32());
                    if (img != null)
                        question.ImagePath = _imageStorage.SaveBytes(img.Bytes, img.MimeType); // own copy per question
                }

                if (item.TryGetProperty("options", out var optionsEl) && optionsEl.ValueKind == JsonValueKind.Array)
                {
                    var optOrder = 1;
                    foreach (var opt in optionsEl.EnumerateArray())
                    {
                        question.Options.Add(new QuizQuestionOption
                        {
                            OptionText = opt.GetProperty("text").GetString() ?? "",
                            IsCorrect = opt.GetProperty("isCorrect").GetBoolean(),
                            DisplayOrder = optOrder++
                        });
                    }
                }

                result.Add(question);
            }

            return result;
        }
    }
}
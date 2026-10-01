// Services/AssessmentServices/AssessmentGenerationService.cs
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Services.AiProviders;
using Tutor_Manager.Services.StudyMaterialTextExtraction;
using Tutor_Manager.Services.Upload;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public class AssessmentGenerationService : IAssessmentGenerationService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAiCompletionOrchestrator _orchestrator;
        private readonly IStudyMaterialTextExtractionService _textExtraction;
        private readonly IAssessmentFileStorageService _fileStorage;
        private readonly ILogger<AssessmentGenerationService> _logger;

        public AssessmentGenerationService(
            Tutor_ManagerDatabaseContext context,
            IAiCompletionOrchestrator orchestrator,
            IStudyMaterialTextExtractionService textExtraction,
            IAssessmentFileStorageService fileStorage,
            ILogger<AssessmentGenerationService> logger)
        {
            _context = context;
            _orchestrator = orchestrator;
            _textExtraction = textExtraction;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        public async Task<QuizGenerationResult> GenerateAsync(int assessmentId, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Files)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TutorUserId == tutorUserId && a.IsActive);

            if (assessment == null)
                return QuizGenerationResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.NotGenerated)
                return QuizGenerationResult.Failure("This assessment has already been generated.");

            var questionPaperFile = assessment.Files.FirstOrDefault(f => f.Role == AssessmentFileRole.QuestionPaper);
            var guidelineFile = assessment.Files.FirstOrDefault(f => f.Role == AssessmentFileRole.MarkingGuideline);

            if (questionPaperFile == null || guidelineFile == null)
                return QuizGenerationResult.Failure("Both the question paper and marking guideline must be present.");

            var questionPaperText = _textExtraction.ExtractFileText(_fileStorage.GetFullPath(questionPaperFile.StoragePath), questionPaperFile.FileType);
            var guidelineText = _textExtraction.ExtractFileText(_fileStorage.GetFullPath(guidelineFile.StoragePath), guidelineFile.FileType);

            if (string.IsNullOrWhiteSpace(questionPaperText))
                return QuizGenerationResult.Failure("Could not extract any text from the question paper.");

            var prompt = BuildPrompt(questionPaperText, guidelineText);

            string responseText;
            try
            {
                responseText = await _orchestrator.CompleteAsync(
                    prompt,
                    new AiCompletionOptions(Temperature: 0.2, MaxOutputTokens: 8192));

                _logger.LogWarning("Raw AI response (Assessment {AssessmentId}): {ResponseText}", assessmentId, responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI assessment generation call failed for assessment {AssessmentId}", assessmentId);
                return QuizGenerationResult.Failure("The AI generation call failed. Please try again.");
            }

            List<AssessmentQuestion> questions;
            try
            {
                questions = ParseResponse(responseText, assessment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse AI assessment response for assessment {AssessmentId}. Raw response length: {Length}",
                    assessmentId, responseText?.Length ?? 0);
                return QuizGenerationResult.Failure("Could not parse the AI response. Please try again.");
            }

            if (!questions.Any())
                return QuizGenerationResult.Failure("The AI did not extract any questions from the question paper.");

            _context.AssessmentQuestions.AddRange(questions);
            assessment.Status = AssessmentGenerationStatus.Generated;
            assessment.GeneratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return QuizGenerationResult.Success();
        }

        private static string BuildPrompt(string questionPaperText, string guidelineText)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are extracting and structuring an existing assessment for a tutoring platform.");
            sb.AppendLine("Do NOT invent new questions. Parse the question paper below into its individual questions, in order, exactly as written.");
            sb.AppendLine("For each question, determine its mark allocation from the question paper if stated, otherwise infer a sensible value from the marking guideline.");
            sb.AppendLine("For each question, write marking guidance summarizing how to award marks for it, based on the marking guideline provided — do not invent marking criteria not supported by the guideline. Match guidance to questions by CONTENT, not by position or order — a question paper and its memo do not always number things identically.");
            sb.AppendLine();
            sb.AppendLine("IGNORE any watermark, download-site, page-header, page-footer, or copyright text scattered through the source (e.g. 'Downloaded from...', 'Copyright reserved', page numbers). Do not include this text anywhere in a question's content.");
            sb.AppendLine();
            sb.AppendLine("QUESTION SPLITTING — each officially numbered sub-part (e.g. 2.1, 2.2, 2.3) becomes exactly one question row. Do not invent additional splits within a single numbered part, and do not merge two distinct numbered parts into one row. Preserve the original numbering (e.g. '2.1') at the start of each question's text, exactly as it appears in the paper, so the mapping back to the original paper stays clear.");
            sb.AppendLine();
            sb.AppendLine("SHARED CONTEXT — each extracted question will be shown to the student as a fully standalone item, with no visibility into any other question or any text that appeared before it in the original paper. If a sub-question depends on shared material — a table, given data, an introductory paragraph, or an instruction that applies to a whole group of questions (e.g. 'Give reasons for your statements in QUESTIONS 9, 10 and 11') — you MUST repeat that shared material in full inside the text of EVERY question that needs it, not just the first one. Never write a question that references 'the table above', 'the diagram above', 'as given', or similar, unless that exact information is also included in that same question's own text.");
            sb.AppendLine();
            sb.AppendLine("DIAGRAMS AND IMAGES — if a question refers to a diagram, graph, image, or figure that is not reproducible as text, do not guess at or invent its contents. Instead, include a short bracketed note in the question text describing what the diagram shows in general terms (e.g. '[Diagram: a right-angled triangle with sides labelled p and t]'), based only on what is stated about it elsewhere in the source text. If nothing at all is stated about the diagram's contents, note plainly that a diagram accompanies the question but its contents are not available in this format.");
            sb.AppendLine();
            sb.AppendLine("For any mathematical, chemical, or physics notation, use LaTeX syntax delimited with single $ for inline math and double $$ for block math. Do not use Markdown formatting (no **bold**, no bullet lists with - or *, no headers) anywhere in the output — the display only renders plain text and LaTeX math, so any other formatting will show as literal stray characters.");
            sb.AppendLine();
            sb.AppendLine("Question paper text:");
            sb.AppendLine("-----");
            sb.AppendLine(questionPaperText);
            sb.AppendLine("-----");
            sb.AppendLine();
            sb.AppendLine("Marking guideline text:");
            sb.AppendLine("-----");
            sb.AppendLine(guidelineText);
            sb.AppendLine("-----");
            sb.AppendLine();
            sb.AppendLine("Respond ONLY with valid JSON in exactly this format, no markdown fences, no other text. Properly escape any quotes or line breaks that occur naturally within question text or marking guidance:");
            sb.AppendLine(@"{
  ""questions"": [
    { ""text"": ""question text here"", ""marks"": 5, ""markingGuidance"": ""guidance text here"" }
  ]
}");
            return sb.ToString();
        }

        private List<AssessmentQuestion> ParseResponse(string responseText, int assessmentId)
        {
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
                responseText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);

            var result = new List<AssessmentQuestion>();
            using var doc = JsonDocument.Parse(responseText);
            var order = 1;

            foreach (var item in doc.RootElement.GetProperty("questions").EnumerateArray())
            {
                result.Add(new AssessmentQuestion
                {
                    AssessmentId = assessmentId,
                    QuestionText = item.GetProperty("text").GetString() ?? "",
                    Marks = item.TryGetProperty("marks", out var marksEl) ? marksEl.GetInt32() : 1,
                    DisplayOrder = order++,
                    MarkingGuidance = item.TryGetProperty("markingGuidance", out var mgEl) ? mgEl.GetString() ?? "" : ""
                });
            }

            return result;
        }
    }
}
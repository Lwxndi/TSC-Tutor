using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tutor_Manager.Models;
using Tutor_Manager.Options;
using Tutor_Manager.Helpers;
using Tutor_Manager.ViewModels.TutorAssignment;

namespace Tutor_Manager.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;
        private readonly ILogger<AiRecommendationService> _logger;

        public AiRecommendationService(
          Tutor_ManagerDatabaseContext context,
          IHttpClientFactory httpClientFactory,
          IOptions<GeminiOptions> options,
          ILogger<AiRecommendationService> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("Gemini");
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AiRecommendationResult> GenerateRecommendationAsync(int tutorUserId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            if (tutor == null)
                throw new InvalidOperationException("Tutor not found.");

            // Correct join: TutorApplication.CreatedTutorId -> Tutor.UserId, not by email
            var application = await _context.TutorApplications
                .Include(a => a.Subjects).ThenInclude(s => s.Subject)
                .Include(a => a.Experience)
                .Include(a => a.Qualifications)
                .FirstOrDefaultAsync(a => a.CreatedTutorId == tutorUserId);

            var activeSubjects = await _context.Subjects
                .Where(s => s.IsActive)
                .Select(s => s.SubjectName)
                .ToListAsync();

            var prompt = BuildPrompt(tutor, application, activeSubjects);

            var responseText = await CallGeminiAsync(prompt);
            var parsed = ParseResponse(responseText, tutorUserId);

            _context.AiRecommendations.Add(new AiRecommendation
            {
                TutorUserId = tutorUserId,
                RecommendationJson = JsonSerializer.Serialize(parsed.Recommendations),
                Reasoning = parsed.Reasoning,
                DateGenerated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return parsed;
        }

        public async Task<List<AiRecommendationResult>> GenerateForTutorsAsync(List<int> tutorUserIds)
        {
            var results = new List<AiRecommendationResult>();
            foreach (var id in tutorUserIds)
            {
                try
                {
                    results.Add(await GenerateRecommendationAsync(id));
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"AI recommendation failed for tutor {id}: {ex.Message}");
                    _logger.LogError(ex, "AI recommendation failed for tutor {TutorUserId}", id);
                }
            }
            return results;
        }

        public async Task<AiRecommendationResult?> GetLatestRecommendationAsync(int tutorUserId)
        {
            var latest = await _context.AiRecommendations
                .Where(r => r.TutorUserId == tutorUserId)
                .OrderByDescending(r => r.DateGenerated)
                .FirstOrDefaultAsync();

            if (latest == null) return null;

            return new AiRecommendationResult
            {
                TutorUserId = tutorUserId,
                Recommendations = JsonSerializer.Deserialize<List<RecommendedSubjectGrade>>(latest.RecommendationJson) ?? new(),
                Reasoning = latest.Reasoning,
                DateGenerated = latest.DateGenerated
            };
        }

        private static string BuildPrompt(Tutor tutor, TutorApplication? application, List<string> activeSubjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are helping a tutoring admin decide what subjects and grades a newly approved tutor should be assigned to teach.");
            sb.AppendLine($"Tutor qualification: {tutor.Qualification ?? "Not specified"}");
            sb.AppendLine($"Tutor bio: {tutor.Bio ?? "Not specified"}");

            if (application != null)
            {
                sb.AppendLine("Achievements: " + (application.Achievements ?? "Not specified"));
                sb.AppendLine("Strengths: " + (application.StrengthsSelected ?? "Not specified"));
                sb.AppendLine("Strengths note: " + (application.StrengthsNote ?? "Not specified"));

                sb.AppendLine("Subjects applied for (with requested grade levels):");
                foreach (var s in application.Subjects)
                {
                    sb.AppendLine($"- {s.Subject.SubjectName}: grades {s.GradeLevels}" +
                                  (string.IsNullOrEmpty(s.CompetencyNote) ? "" : $" (competency: {s.CompetencyNote})") +
                                  (string.IsNullOrEmpty(s.ResultNote) ? "" : $" (results: {s.ResultNote})"));
                }

                sb.AppendLine("Teaching/work experience:");
                foreach (var e in application.Experience)
                {
                    sb.AppendLine($"- {e.Institution}: taught {e.SubjectsTaught ?? "unspecified subjects"}, " +
                                  $"grades {e.GradeLevels ?? "unspecified"}, duration {e.Duration ?? "unspecified"}. " +
                                  $"Responsibilities: {e.Responsibilities ?? "none listed"}. Achievements: {e.Achievements ?? "none listed"}.");
                }

                sb.AppendLine("Qualifications:");
                foreach (var q in application.Qualifications)
                {
                    sb.AppendLine($"- {q.QualificationType} in {q.FieldOfStudy ?? "unspecified field"} from " +
                                  $"{q.Institution ?? "unspecified institution"} ({q.StudyStatus}" +
                                  (q.YearCompleted.HasValue ? $", completed {q.YearCompleted}" : "") + ")");
                }
            }

            sb.AppendLine($"TSC currently offers these subjects: {string.Join(", ", activeSubjects)}");
            sb.AppendLine("Grades are 10, 11, and 12 only.");
            sb.AppendLine();
            sb.AppendLine("Respond ONLY with valid JSON in exactly this format, no markdown fences, no other text:");
            sb.AppendLine(@"{ ""recommendations"": [ { ""subjectName"": ""Mathematics"", ""grade"": 10 } ], ""reasoning"": ""short explanation"" }");

            return sb.ToString();
        }

        private async Task<string> CallGeminiAsync(string prompt)
        {
            _logger.LogWarning("Gemini API key length: {Length}", _options.ApiKey?.Length ?? 0);
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
         

            var responseJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini API error {(int)response.StatusCode}: {responseJson}");

            using var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "{}";

            // Gemini sometimes wraps JSON in ```json fences despite instructions — strip them defensively
            return text.Replace("```json", "").Replace("```", "").Trim();
        }

        private AiRecommendationResult ParseResponse(string responseText, int tutorUserId)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                var recommendations = new List<RecommendedSubjectGrade>();
                foreach (var item in root.GetProperty("recommendations").EnumerateArray())
                {
                    var subjectName = item.GetProperty("subjectName").GetString() ?? "";
                    var grade = item.GetProperty("grade").GetInt32();

                    var subject = _context.Subjects.FirstOrDefault(s => s.SubjectName == subjectName);

                    recommendations.Add(new RecommendedSubjectGrade
                    {
                        SubjectId = subject?.SubjectId ?? 0,
                        SubjectName = subjectName,
                        Grade = grade
                    });
                }

                return new AiRecommendationResult
                {
                    TutorUserId = tutorUserId,
                    Recommendations = recommendations,
                    Reasoning = root.GetProperty("reasoning").GetString() ?? "",
                    DateGenerated = DateTime.Now
                };
            }
            catch (Exception)
            {
                return new AiRecommendationResult
                {
                    TutorUserId = tutorUserId,
                    Recommendations = new(),
                    Reasoning = "Could not parse AI response. Please try again.",
                    DateGenerated = DateTime.Now
                };
            }
        }
    }
}
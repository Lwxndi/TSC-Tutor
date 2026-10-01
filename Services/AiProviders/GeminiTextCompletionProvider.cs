////using Microsoft.Extensions.Options;
////using System.Text;
////using System.Text.Json;
////using Tutor_Manager.Options;
////using Tutor_Manager.Services.AiProviders;
//////using Tutor_Manager.Services.AiCompletion;
//////using Tutor_Manager.Services.AssessmentServices;
//////using Tutor_Manager.Services.QuizzServices;
//////using Tutor_Manager.Services.StudyMaterialTextExtraction;

////namespace Tutor_Manager.Services.AiCompletion
////{
////    public class GeminiTextCompletionProvider : IAiTextCompletionProvider
////    {
////        private readonly HttpClient _httpClient;
////        private readonly GeminiOptions _options;

////        public string Name => "Gemini";

////        public GeminiTextCompletionProvider(IHttpClientFactory httpClientFactory, IOptions<GeminiOptions> options)
////        {
////            _httpClient = httpClientFactory.CreateClient("Gemini");
////            _options = options.Value;
////        }

////        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken)
////        {
////            if (string.IsNullOrWhiteSpace(_options.ApiKey))
////                throw new InvalidOperationException("Gemini API key is not configured.");

////            var requestBody = new
////            {
////                contents = new[] { new { parts = new[] { new { text = prompt } } } },
////                generationConfig = new { temperature = options.Temperature, maxOutputTokens = options.MaxOutputTokens }
////            };

////            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";
////            var request = new HttpRequestMessage(HttpMethod.Post, url)
////            {
////                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
////            };

////            var response = await _httpClient.SendAsync(request, cancellationToken);
////            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

////            if (!response.IsSuccessStatusCode)
////                throw new InvalidOperationException($"Gemini API error {(int)response.StatusCode}: {responseJson}");

////            using var doc = JsonDocument.Parse(responseJson);
////            var text = doc.RootElement
////                .GetProperty("candidates")[0]
////                .GetProperty("content")
////                .GetProperty("parts")[0]
////                .GetProperty("text")
////                .GetString() ?? "{}";

////            return text.Replace("```json", "").Replace("```", "").Trim();
////        }
////    }
////}

//using Microsoft.Extensions.Options;
//using System.Text;
//using System.Text.Json;
//using Tutor_Manager.Options;
//using Tutor_Manager.Services.AiProviders;

//namespace Tutor_Manager.Services.AiCompletion
//{
//    public class GeminiTextCompletionProvider : IAiTextCompletionProvider
//    {
//        private readonly HttpClient _httpClient;
//        private readonly GeminiOptions _options;

//        public string Name => "Gemini";

//        public GeminiTextCompletionProvider(IHttpClientFactory httpClientFactory, IOptions<GeminiOptions> options)
//        {
//            _httpClient = httpClientFactory.CreateClient("Gemini");
//            _options = options.Value;
//        }

//        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrWhiteSpace(_options.ApiKey))
//                throw new InvalidOperationException("Gemini API key is not configured.");

//            var requestBody = new
//            {
//                contents = new[] { new { parts = new[] { new { text = prompt } } } },
//                generationConfig = new { temperature = 0.4, maxOutputTokens = 2048 }
//            };

//            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";
//            var request = new HttpRequestMessage(HttpMethod.Post, url)
//            {
//                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
//            };

//            var response = await _httpClient.SendAsync(request, cancellationToken);
//            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

//            if (!response.IsSuccessStatusCode)
//                throw new InvalidOperationException($"Gemini API error {(int)response.StatusCode}: {responseJson}");

//            using var doc = JsonDocument.Parse(responseJson);
//            var text = doc.RootElement
//                .GetProperty("candidates")[0]
//                .GetProperty("content")
//                .GetProperty("parts")[0]
//                .GetProperty("text")
//                .GetString() ?? "{}";

//            return text.Replace("```json", "").Replace("```", "").Trim();
//        }
//    }
//}

using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Tutor_Manager.Options;
using Tutor_Manager.Services.AiProviders;

namespace Tutor_Manager.Services.AiCompletion
{
    public class GeminiTextCompletionProvider : IAiTextCompletionProvider
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;
        private readonly ILogger<GeminiTextCompletionProvider> _logger;

        public string Name => "Gemini";

        public GeminiTextCompletionProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<GeminiOptions> options,
            ILogger<GeminiTextCompletionProvider> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Gemini");
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("Gemini API key is not configured.");

            var generationConfig = new Dictionary<string, object>
            {
                ["temperature"] = options.Temperature,
                ["maxOutputTokens"] = options.MaxOutputTokens
            };

            if (options.JsonOutput)
                generationConfig["responseMimeType"] = "application/json";

            if (_options.ThinkingBudget.HasValue)
                generationConfig["thinkingConfig"] = new { thinkingBudget = _options.ThinkingBudget.Value };
            else if (!string.IsNullOrWhiteSpace(_options.ThinkingLevel))
                generationConfig["thinkingConfig"] = new { thinkingLevel = _options.ThinkingLevel };

            var requestParts = new List<object>();
            if (options.Images is { Count: > 0 })
            {
                foreach (var img in options.Images)
                {
                    requestParts.Add(new { text = $"Image {img.Ref}:" });
                    requestParts.Add(new { inlineData = new { mimeType = img.MimeType, data = Convert.ToBase64String(img.Bytes) } });
                }
            }
            requestParts.Add(new { text = prompt });

            var requestBody = new
            {
                contents = new[] { new { parts = requestParts } },
                generationConfig
            };

            // Key goes in a header instead of the URL so it can't leak into logs.
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-goog-api-key", _options.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini API error {(int)response.StatusCode}: {responseJson}");

            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                throw new InvalidOperationException($"Gemini returned no candidates: {responseJson}");

            var candidate = candidates[0];
            var finishReason = candidate.TryGetProperty("finishReason", out var fr) ? fr.GetString() : null;

            // Join ALL text parts, skipping thinking parts.
            var sb = new StringBuilder();
            var partCount = 0;
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    partCount++;
                    if (part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True)
                        continue;
                    if (part.TryGetProperty("text", out var textEl))
                        sb.Append(textEl.GetString());
                }
            }

            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                _logger.LogInformation(
                    "Gemini finished: reason={FinishReason}, parts={Parts}, promptTokens={Prompt}, outputTokens={Output}, thinkingTokens={Thinking}",
                    finishReason, partCount,
                    usage.TryGetProperty("promptTokenCount", out var p) ? p.GetInt32() : -1,
                    usage.TryGetProperty("candidatesTokenCount", out var c) ? c.GetInt32() : -1,
                    usage.TryGetProperty("thoughtsTokenCount", out var t) ? t.GetInt32() : 0);
            }

            // A truncated response is a failure, not a success. Throwing lets the orchestrator fall back.
            if (finishReason == "MAX_TOKENS")
                throw new InvalidOperationException("Gemini response was truncated (MAX_TOKENS).");

            if (finishReason is not (null or "STOP"))
                throw new InvalidOperationException($"Gemini stopped with finishReason={finishReason}.");

            var text = sb.ToString();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini returned empty text.");

            return text.Replace("```json", "").Replace("```", "").Trim();
        }
    }
}
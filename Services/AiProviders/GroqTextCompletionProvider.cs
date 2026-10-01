//using Microsoft.Extensions.Options;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using Tutor_Manager.Options;
//using Tutor_Manager.Services.AiProviders;

//namespace Tutor_Manager.Services.AiCompletion
//{
//    public class GroqTextCompletionProvider : IAiTextCompletionProvider
//    {
//        private readonly HttpClient _httpClient;
//        private readonly GroqOptions _options;

//        public string Name => "Groq";

//        public GroqTextCompletionProvider(IHttpClientFactory httpClientFactory, IOptions<GroqOptions> options)
//        {
//            _httpClient = httpClientFactory.CreateClient("Groq");
//            _options = options.Value;
//        }

//        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrWhiteSpace(_options.ApiKey))
//                throw new InvalidOperationException("Groq API key is not configured.");

//            var requestBody = new
//            {
//                model = _options.Model,
//                messages = new[] { new { role = "user", content = prompt } },
//                temperature = options.Temperature,
//                max_tokens = options.MaxOutputTokens
//            };

//            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
//            {
//                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
//            };
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

//            var response = await _httpClient.SendAsync(request, cancellationToken);
//            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

//            if (!response.IsSuccessStatusCode)
//                throw new InvalidOperationException($"Groq API error {(int)response.StatusCode}: {responseJson}");

//            using var doc = JsonDocument.Parse(responseJson);
//            var text = doc.RootElement
//                .GetProperty("choices")[0]
//                .GetProperty("message")
//                .GetProperty("content")
//                .GetString() ?? "{}";

//            return text.Replace("```json", "").Replace("```", "").Trim();
//        }

//        //public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken)
//        //{
//        //    // Dormant until a key is configured — fails fast rather than sending an
//        //    // unauthenticated request, so this stays a no-op until Groq:ApiKey is set.
//        //    if (string.IsNullOrWhiteSpace(_options.ApiKey))
//        //        throw new InvalidOperationException("Groq API key is not configured.");

//        //    var requestBody = new
//        //    {
//        //        model = _options.Model,
//        //        messages = new[] { new { role = "user", content = prompt } },
//        //        temperature = 0.4
//        //    };

//        //    // Groq's API is OpenAI-compatible (same request/response shape as
//        //    // /v1/chat/completions), just a different base URL and model catalog.
//        //    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
//        //    {
//        //        Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
//        //    };
//        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

//        //    var response = await _httpClient.SendAsync(request, cancellationToken);
//        //    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

//        //    if (!response.IsSuccessStatusCode)
//        //        throw new InvalidOperationException($"Groq API error {(int)response.StatusCode}: {responseJson}");

//        //    using var doc = JsonDocument.Parse(responseJson);
//        //    var text = doc.RootElement
//        //        .GetProperty("choices")[0]
//        //        .GetProperty("message")
//        //        .GetProperty("content")
//        //        .GetString() ?? "{}";

//        //    return text.Replace("```json", "").Replace("```", "").Trim();
//        //}
//    }
//}
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tutor_Manager.Options;
using Tutor_Manager.Services.AiProviders;

namespace Tutor_Manager.Services.AiCompletion
{
    public class GroqTextCompletionProvider : IAiTextCompletionProvider
    {
        private readonly HttpClient _httpClient;
        private readonly GroqOptions _options;
        private readonly ILogger<GroqTextCompletionProvider> _logger;
        public string Name => "Groq";
        private const int GroqTpmLimit = 8000;
        private const int SafetyMarginTokens = 300;
        private const int MinPromptTokens = 3000; // guarantee real room for instructions + material
        private const int MaxGroqCompletionTokens = GroqTpmLimit - SafetyMarginTokens - MinPromptTokens; // 4700
        private const double ApproxCharsPerToken = 4.0;

        public GroqTextCompletionProvider(IHttpClientFactory httpClientFactory, IOptions<GroqOptions> options, ILogger<GroqTextCompletionProvider> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Groq");
            _options = options.Value;
            _logger = logger;
        }

        //public async Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken)
        //{
        //    if (string.IsNullOrWhiteSpace(_options.ApiKey))
        //        throw new InvalidOperationException("Groq API key is not configured.");

        //    var requestBody = new Dictionary<string, object>
        //    {
        //        ["model"] = _options.Model,
        //        ["messages"] = new[] { new { role = "user", content = prompt } },
        //        ["temperature"] = options.Temperature,
        //        ["max_tokens"] = options.MaxOutputTokens
        //    };
        //    if (options.JsonOutput)
        //        requestBody["response_format"] = new { type = "json_object" };

        //    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        //    {
        //        Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        //    };
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        //    var response = await _httpClient.SendAsync(request, cancellationToken);
        //    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        //    if (!response.IsSuccessStatusCode)
        //        throw new InvalidOperationException($"Groq API error {(int)response.StatusCode}: {responseJson}");

        //    using var doc = JsonDocument.Parse(responseJson);
        //    var choice = doc.RootElement.GetProperty("choices")[0];
        //    var finishReason = choice.TryGetProperty("finish_reason", out var fr) ? fr.GetString() : null;

        //    if (finishReason == "length")
        //        throw new InvalidOperationException("Groq response was truncated (length).");

        //    var text = choice.GetProperty("message").GetProperty("content").GetString();
        //    if (string.IsNullOrWhiteSpace(text))
        //        throw new InvalidOperationException("Groq returned empty text.");

        //    return text.Replace("```json", "").Replace("```", "").Trim();
        //}


        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("Groq API key is not configured.");

            // Groq's on_demand tier caps this model at 8000 tokens/request (prompt + completion).
            // Gemini has no such cap, so this clamp is Groq-only and never touches the primary path.
            int effectiveMaxOutputTokens = Math.Min(options.MaxOutputTokens, MaxGroqCompletionTokens);
            if (effectiveMaxOutputTokens < options.MaxOutputTokens)
            {
                _logger.LogWarning(
                    "Reducing Groq max_tokens from {Requested} to {Effective} to leave room for the prompt under Groq's {Limit}-token cap.",
                    options.MaxOutputTokens, effectiveMaxOutputTokens, GroqTpmLimit);
            }

            prompt = TrimForGroqBudget(prompt, effectiveMaxOutputTokens);

            var requestBody = new Dictionary<string, object>
            {
                ["model"] = _options.Model,
                ["messages"] = new[] { new { role = "user", content = prompt } },
                ["temperature"] = options.Temperature,
                ["max_tokens"] = effectiveMaxOutputTokens
            };
            if (options.JsonOutput)
                requestBody["response_format"] = new { type = "json_object" };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Groq API error {(int)response.StatusCode}: {responseJson}");

            using var doc = JsonDocument.Parse(responseJson);
            var choice = doc.RootElement.GetProperty("choices")[0];
            var finishReason = choice.TryGetProperty("finish_reason", out var fr) ? fr.GetString() : null;

            if (finishReason == "length")
                throw new InvalidOperationException("Groq response was truncated (length).");

            var text = choice.GetProperty("message").GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Groq returned empty text.");

            return text.Replace("```json", "").Replace("```", "").Trim();
        }

        private string TrimForGroqBudget(string prompt, int effectiveMaxOutputTokens)
        {
            int budgetTokens = GroqTpmLimit - effectiveMaxOutputTokens - SafetyMarginTokens;
            if (budgetTokens < MinPromptTokens)
                budgetTokens = MinPromptTokens; // floor, guaranteed by the completion cap above

            int budgetChars = (int)(budgetTokens * ApproxCharsPerToken);

            if (prompt.Length <= budgetChars)
                return prompt;

            _logger.LogWarning(
                "Trimming prompt for Groq: {OriginalChars} chars (~{EstTokens} est. tokens) exceeds budget of {BudgetChars} chars (~{BudgetTokens} tokens). Truncating from the end.",
                prompt.Length, (int)(prompt.Length / ApproxCharsPerToken), budgetChars, budgetTokens);

            return prompt.Substring(0, budgetChars)
                + "\n\n[Note: source material was truncated to fit Groq's token limit.]";
        }
    }
}
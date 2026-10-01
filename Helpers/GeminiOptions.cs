namespace Tutor_Manager.Options
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = null!;
        public string Model { get; set; } = "gemini-2.0-flash";

        // Optional. Set one of these in appsettings to stop thinking tokens eating the output cap.
        public int? ThinkingBudget { get; set; }      // e.g. 512 (Gemini 2.5-style)
        public string? ThinkingLevel { get; set; }    // e.g. "low" (Gemini 3-style)
    }
}
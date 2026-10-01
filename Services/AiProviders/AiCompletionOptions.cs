// Services/AiProviders/AiCompletionOptions.cs
namespace Tutor_Manager.Services.AiProviders
{
    // AiCompletionOptions.cs
    public record AiImage(int Ref, string MimeType, byte[] Bytes);
    public record AiCompletionOptions(
        double Temperature = 0.4,
        int MaxOutputTokens = 2048,
        bool JsonOutput = false,
        IReadOnlyList<AiImage>? Images = null);
}
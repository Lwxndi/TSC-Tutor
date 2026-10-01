// Services/AiProviders/IAiCompletionProvider.cs
namespace Tutor_Manager.Services.AiProviders
{
    public interface IAiTextCompletionProvider
    {
        string Name { get; }
        Task<string> CompleteAsync(string prompt, AiCompletionOptions options, CancellationToken cancellationToken);
    }
}
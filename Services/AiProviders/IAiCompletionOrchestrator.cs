// Services/AiProviders/IHedgedAiCompletionService.cs
namespace Tutor_Manager.Services.AiProviders
{
    public interface IAiCompletionOrchestrator
    {
        Task<string> CompleteAsync(string prompt, AiCompletionOptions? options = null);
    }
}
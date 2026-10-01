// Services/AiProviders/AiCallOutcome.cs
namespace Tutor_Manager.Services.AiProviders
{
    internal class AiCallOutcome
    {
        public bool Succeeded { get; private set; }
        public string? Response { get; private set; }
        public string ProviderName { get; private set; } = "";
        public Exception? Error { get; private set; }

        public static AiCallOutcome Success(string response, string providerName) =>
            new() { Succeeded = true, Response = response, ProviderName = providerName };

        public static AiCallOutcome Failure(Exception error, string providerName) =>
            new() { Succeeded = false, Error = error, ProviderName = providerName };
    }
}
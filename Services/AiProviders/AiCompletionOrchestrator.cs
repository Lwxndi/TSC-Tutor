//using Tutor_Manager.Services.AiProviders;

//namespace Tutor_Manager.Services.AiCompletion
//{
//    public class AiCompletionOrchestrator : IAiCompletionOrchestrator
//    {
//        private readonly GeminiTextCompletionProvider _primary;
//        private readonly GroqTextCompletionProvider _fallback;
//        private readonly ILogger<AiCompletionOrchestrator> _logger;

//        // Confirmed values: hedge at 8s, hard cap at 20s total.
//        private static readonly TimeSpan HedgeDelay = TimeSpan.FromSeconds(8);
//        private static readonly TimeSpan OverallTimeout = TimeSpan.FromSeconds(20);

//        public AiCompletionOrchestrator(
//            GeminiTextCompletionProvider primary,
//            GroqTextCompletionProvider fallback,
//            ILogger<AiCompletionOrchestrator> logger)
//        {
//            _primary = primary;
//            _fallback = fallback;
//            _logger = logger;
//        }

//        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions? options = null)
//        {
//            var resolvedOptions = options ?? new AiCompletionOptions();

//            using var overallCts = new CancellationTokenSource(OverallTimeout);
//            using var primaryCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
//            using var fallbackCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);

//            var primaryTask = RunProviderAsync(_primary, prompt, resolvedOptions, primaryCts.Token);
//            var hedgeDelayTask = Task.Delay(HedgeDelay, overallCts.Token);

//            // Race: does the primary finish (success or failure) before the hedge delay elapses?
//            var firstSignal = await Task.WhenAny((Task)primaryTask, hedgeDelayTask);

//            if (firstSignal == primaryTask && primaryTask.Result.Success)
//            {
//                // Primary succeeded within the hedge window — fallback never starts.
//                return primaryTask.Result.Text!;
//            }

//            if (firstSignal == primaryTask)
//            {
//                _logger.LogWarning("Primary AI provider ({Provider}) failed before the hedge delay; starting fallback immediately.", _primary.Name);
//            }
//            else
//            {
//                _logger.LogWarning("Primary AI provider had not responded within {Seconds}s; racing fallback provider alongside it.", HedgeDelay.TotalSeconds);
//            }

//            // Either the primary failed outright, or the hedge delay elapsed with it still running.
//            // Start the fallback now; both may be in flight simultaneously.
//            var fallbackTask = RunProviderAsync(_fallback, prompt, resolvedOptions, fallbackCts.Token);

//            var pending = new List<Task<ProviderResult>> { primaryTask, fallbackTask };

//            while (pending.Count > 0)
//            {
//                var completed = await Task.WhenAny(pending);
//                pending.Remove(completed);

//                if (completed.Result.Success)
//                {
//                    // Whichever wins, cancel the other — it's discarded silently,
//                    // never surfaced to the caller which provider actually answered.
//                    primaryCts.Cancel();
//                    fallbackCts.Cancel();
//                    return completed.Result.Text!;
//                }
//            }

//            // Both failed, or the overall timeout fired and cancelled whichever was still running.
//            throw new InvalidOperationException("Both the primary and fallback AI providers failed or timed out.");
//        }

//        private async Task<ProviderResult> RunProviderAsync(IAiTextCompletionProvider provider, string prompt, AiCompletionOptions options, CancellationToken ct)
//        {
//            try
//            {
//                var text = await provider.CompleteAsync(prompt, options, ct);
//                return new ProviderResult(true, text, provider.Name);
//            }
//            catch (OperationCanceledException)
//            {
//                // Cancelled either by losing the race or by the overall timeout — not a real error, just a loss.
//                return new ProviderResult(false, null, provider.Name);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "AI provider {Provider} failed", provider.Name);
//                return new ProviderResult(false, null, provider.Name);
//            }
//        }

//        private record ProviderResult(bool Success, string? Text, string ProviderName);
//    }
//}

using Tutor_Manager.Services.AiProviders;

namespace Tutor_Manager.Services.AiCompletion
{
    public class AiCompletionOrchestrator : IAiCompletionOrchestrator
    {
        private readonly GeminiTextCompletionProvider _primary;
        private readonly GroqTextCompletionProvider _fallback;
        private readonly ILogger<AiCompletionOrchestrator> _logger;

        // Confirmed values: hedge at 8s, hard cap at 20s total.
        private static readonly TimeSpan HedgeDelay = TimeSpan.FromSeconds(8);
        private static readonly TimeSpan OverallTimeout = TimeSpan.FromSeconds(20);

        public AiCompletionOrchestrator(
            GeminiTextCompletionProvider primary,
            GroqTextCompletionProvider fallback,
            ILogger<AiCompletionOrchestrator> logger)
        {
            _primary = primary;
            _fallback = fallback;
            _logger = logger;
        }

        public async Task<string> CompleteAsync(string prompt, AiCompletionOptions? options = null)
        {
            var resolvedOptions = options ?? new AiCompletionOptions();

            using var overallCts = new CancellationTokenSource(OverallTimeout);
            using var primaryCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
            using var fallbackCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);

            var primaryTask = RunProviderAsync(_primary, prompt, resolvedOptions, primaryCts.Token);
            var hedgeDelayTask = Task.Delay(HedgeDelay, overallCts.Token);

            // Race: does the primary finish (success or failure) before the hedge delay elapses?
            var firstSignal = await Task.WhenAny((Task)primaryTask, hedgeDelayTask);

            if (firstSignal == primaryTask && primaryTask.Result.Success)
            {
                // Primary succeeded within the hedge window — fallback never starts.
                return primaryTask.Result.Text!;
            }

            if (firstSignal == primaryTask)
            {
                _logger.LogWarning("Primary AI provider ({Provider}) failed before the hedge delay; starting fallback immediately.", _primary.Name);
            }
            else
            {
                _logger.LogWarning("Primary AI provider had not responded within {Seconds}s; racing fallback provider alongside it.", HedgeDelay.TotalSeconds);
            }

            // Either the primary failed outright, or the hedge delay elapsed with it still running.
            // Start the fallback now; both may be in flight simultaneously.
            var fallbackTask = RunProviderAsync(_fallback, prompt, resolvedOptions, fallbackCts.Token);

            var pending = new List<Task<ProviderResult>> { primaryTask, fallbackTask };

            while (pending.Count > 0)
            {
                var completed = await Task.WhenAny(pending);
                pending.Remove(completed);

                if (completed.Result.Success)
                {
                    // Whichever wins, cancel the other — it's discarded silently,
                    // never surfaced to the caller which provider actually answered.
                    primaryCts.Cancel();
                    fallbackCts.Cancel();
                    return completed.Result.Text!;
                }
            }

            // Both failed, or the overall timeout fired and cancelled whichever was still running.
            throw new InvalidOperationException("Both the primary and fallback AI providers failed or timed out.");
        }

        private async Task<ProviderResult> RunProviderAsync(IAiTextCompletionProvider provider, string prompt, AiCompletionOptions options, CancellationToken ct)
        {
            try
            {
                var text = await provider.CompleteAsync(prompt, options, ct);
                return new ProviderResult(true, text, provider.Name);
            }
            catch (OperationCanceledException)
            {
                // Cancelled either by losing the race or by the overall timeout — not a real error, just a loss.
                return new ProviderResult(false, null, provider.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI provider {Provider} failed", provider.Name);
                return new ProviderResult(false, null, provider.Name);
            }
        }

        private record ProviderResult(bool Success, string? Text, string ProviderName);
    }
}
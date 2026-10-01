namespace Tutor_Manager.Options
{
    public class StripeOptions
    {
        // Never in appsettings.json — set via user secrets (dotnet user-secrets set
        // "Stripe:SecretKey" "sk_test_..."), same pattern as Gemini:ApiKey.
        public string SecretKey { get; set; } = null!;
        public string PublishableKey { get; set; } = null!;
        public string WebhookSecret { get; set; } = null!;
    }
}
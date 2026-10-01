namespace Tutor_Manager.Models
{
    public class CheckoutSessionResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public string? CheckoutUrl { get; set; }

        public static CheckoutSessionResult Success(string url) =>
            new() { Succeeded = true, CheckoutUrl = url };

        public static CheckoutSessionResult Failure(string error) =>
            new() { Succeeded = false, ErrorMessage = error };
    }
}
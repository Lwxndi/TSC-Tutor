namespace Tutor_Manager.Models
{
    public class MarkInvoicePaidResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }

        // Deliberately NOT restored here — PaymentService has no dependency on
        // IEnrollmentService (would create a circular dependency, since
        // EnrollmentService already depends on IPaymentService for invoice
        // generation). The caller (StripeWebhookController) restores each of
        // these via IEnrollmentService.RestoreFromSuspensionAsync after this
        // returns.
        public List<int> EnrollmentIdsToRestore { get; set; } = new();

        public static MarkInvoicePaidResult Success(List<int> toRestore) =>
            new() { Succeeded = true, EnrollmentIdsToRestore = toRestore };

        public static MarkInvoicePaidResult Failure(string error) =>
            new() { Succeeded = false, ErrorMessage = error };
    }
}
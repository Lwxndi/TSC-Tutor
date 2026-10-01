using Tutor_Manager.Models.Enums;

// REPLACES the old result classes (PaymentResult, RecurringBillingResult, CheckoutSessionResult,
// MarkInvoicePaidResult). Delete the old definitions wherever they live.
namespace Tutor_Manager.Services.PaymentServices
{
    public class PaymentResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public int? InvoiceId { get; set; }

        public static PaymentResult Success(int invoiceId) => new() { Succeeded = true, InvoiceId = invoiceId };
        public static PaymentResult Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
    }

    public class RecurringBillingResult
    {
        public int Generated { get; set; }
        public int Skipped { get; set; }
        public int CarriedForwardLines { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CheckoutSessionResult
    {
        public bool Succeeded { get; set; }
        public string? CheckoutUrl { get; set; }
        public int? PaymentId { get; set; }
        public string? ErrorMessage { get; set; }

        public static CheckoutSessionResult Success(string url, int paymentId) =>
            new() { Succeeded = true, CheckoutUrl = url, PaymentId = paymentId };
        public static CheckoutSessionResult Failure(string error) =>
            new() { Succeeded = false, ErrorMessage = error };
    }

    // Outcome of trying to apply a confirmed Stripe payment to our records.
    public class PaymentApplyResult
    {
        public bool Succeeded { get; set; }
        public bool AlreadyProcessed { get; set; }   // another caller (webhook / success page) got there first
        public int? PaymentId { get; set; }
        public string? ErrorMessage { get; set; }

        public static PaymentApplyResult Applied(int paymentId) =>
            new() { Succeeded = true, PaymentId = paymentId };
        public static PaymentApplyResult Already(int paymentId) =>
            new() { Succeeded = true, AlreadyProcessed = true, PaymentId = paymentId };
        public static PaymentApplyResult Failure(string error, int? paymentId = null) =>
            new() { Succeeded = false, ErrorMessage = error, PaymentId = paymentId };
    }

    // Used by the Success page.
    public class PaymentVerificationResult
    {
        public bool Found { get; set; }
        public int? PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
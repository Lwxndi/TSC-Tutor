namespace Tutor_Manager.Models
{
    public class PaymentResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public int? InvoiceId { get; set; }

        public static PaymentResult Success(int invoiceId) =>
            new() { Succeeded = true, InvoiceId = invoiceId };

        public static PaymentResult Failure(string error) =>
            new() { Succeeded = false, ErrorMessage = error };
    }
}
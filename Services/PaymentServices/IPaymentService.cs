using Tutor_Manager.Models;

namespace Tutor_Manager.Services.PaymentServices
{
    public interface IPaymentService
    {
        // ---------- Invoice creation ----------

        // First invoice for an enrollment that is AwaitingPayment. One invoice, one line item,
        // priced at the Offering's current price, due today.
        Task<PaymentResult> GenerateFirstInvoiceAsync(int enrollmentId, int guardianUserId);

        // Enrollment was redirected while AwaitingPayment: void the unpaid invoice and issue a
        // new one at the new Offering's price, for the same guardian.
        Task<PaymentResult> RegenerateFirstInvoiceAsync(int enrollmentId);

        // Voids every Open invoice for the enrollment (used when an AwaitingPayment enrollment
        // is cancelled or withdrawn). Any in-flight Stripe session for them is expired first.
        // Returns how many invoices were voided.
        Task<int> VoidUnpaidInvoicesForEnrollmentAsync(int enrollmentId, string reason);

        // Manual admin trigger. One new invoice per due Active+Recurring enrollment; any unpaid
        // earlier balance for that enrollment is carried forward into it.
        Task<RecurringBillingResult> GenerateDueRecurringInvoicesAsync();

        // ---------- Paying ----------

        // One Stripe Checkout Session covering one or more of the guardian's invoices.
        // successUrlTemplate must contain the literal "{CHECKOUT_SESSION_ID}".
        Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
            IReadOnlyCollection<int> invoiceIds, int guardianUserId, string successUrlTemplate, string cancelUrl);

        // Success page: asks Stripe directly and applies the payment if it is paid and the
        // webhook has not already done so. Safe to call any number of times.
        Task<PaymentVerificationResult> VerifyCheckoutSessionAsync(string sessionId, int guardianUserId);

        // ---------- Webhook entry points ----------

        // checkout.session.completed (payment_status == paid) and async_payment_succeeded.
        Task<PaymentApplyResult> ProcessPaidSessionAsync(Stripe.Checkout.Session session);
        Task HandleSessionExpiredAsync(Stripe.Checkout.Session session);
        Task HandleSessionFailedAsync(Stripe.Checkout.Session session);

        // ---------- Reads (all AsNoTracking) ----------

        Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
        Task<List<Invoice>> GetInvoicesForGuardianAsync(int guardianUserId);
        Task<List<Invoice>> GetPayableInvoicesForGuardianAsync(int guardianUserId);
        Task<List<Invoice>> GetAllInvoicesAsync();

        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<List<Payment>> GetPaidPaymentsForGuardianAsync(int guardianUserId);
        Task<List<Payment>> GetAllPaymentsAsync(string? search = null);
    }
}
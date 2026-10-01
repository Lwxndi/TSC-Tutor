using Tutor_Manager.Models;

namespace Tutor_Manager.Services.ReceiptServices
{
    public interface IReceiptService
    {
        // Invoice PDF (unpaid, or to re-read one). Needs: Guardian.User, LineItems.Learner.User,
        // LineItems.OfferingSnapshot.Subject — IPaymentService.GetInvoiceByIdAsync loads all of it.
        byte[] GenerateInvoicePdf(Invoice invoice);

        // Payment receipt: one per Stripe payment, covering every invoice it paid, every learner.
        // adminView adds the extra payment-details section. Needs IPaymentService.GetPaymentByIdAsync.
        byte[] GeneratePaymentReceiptPdf(Payment payment, bool adminView);

        string GetInvoiceReference(Invoice invoice);   // INV-000003
        string GetReceiptReference(Payment payment);   // RCT-000012
        string GetInvoiceFileName(Invoice invoice);    // Invoice-INV-000003.pdf
        string GetReceiptFileName(Payment payment);    // Receipt-RCT-000012.pdf
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    // One row per Stripe Checkout attempt. A single Payment can cover several invoices
    // (a guardian paying for two children in one go). Created BEFORE the guardian is sent
    // to Stripe, so nothing is ever "unknown" about an in-flight payment.
    public class Payment
    {
        public int PaymentId { get; set; }

        [ForeignKey("Guardian")]
        public int GuardianUserId { get; set; }
        public Parent Guardian { get; set; } = null!;

        public decimal Amount { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "ZAR";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [StringLength(255)]
        public string? StripeCheckoutSessionId { get; set; }

        [StringLength(255)]
        public string? StripePaymentIntentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Set when something about the payment needs a human (amount mismatch, invoice was
        // voided while the guardian was paying, etc.).
        [StringLength(1000)]
        public string? ReviewNote { get; set; }

        public ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();
    }

    public class PaymentInvoice
    {
        public int PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        // Balance of the invoice at the moment the payment was created.
        public decimal AmountApplied { get; set; }
    }
}
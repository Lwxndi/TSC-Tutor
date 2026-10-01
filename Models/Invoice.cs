using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [ForeignKey("Guardian")]
        public int GuardianUserId { get; set; }
        public Parent Guardian { get; set; } = null!;

        // Sums of LineItems — stored for fast dashboard/reporting reads.
        public decimal TotalAmountDue { get; set; }
        public decimal TotalAmountPaidSoFar { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

        public string? StripePaymentIntentId { get; set; }
        public string? StripeInvoiceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaidAt { get; set; }

        // Set when Status == Voided.
        public DateTime? VoidedAt { get; set; }

        [StringLength(300)]
        public string? VoidReason { get; set; }

        // Set when Status == CarriedForward: the newer invoice that absorbed this one's balance.
        public int? CarriedForwardIntoInvoiceId { get; set; }
        public Invoice? CarriedForwardIntoInvoice { get; set; }

        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        public ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();
    }
}
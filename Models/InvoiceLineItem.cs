using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class InvoiceLineItem
    {
        public int InvoiceLineItemId { get; set; }

        [ForeignKey("Invoice")]
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        [ForeignKey("Enrollment")]
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        // FK to the Offering as it was when billed (the Offering row itself can be edited later;
        // Description below is the frozen text).
        [ForeignKey("OfferingSnapshot")]
        public int OfferingSnapshotId { get; set; }
        public Offering OfferingSnapshot { get; set; } = null!;

        // Frozen text of what was billed ("Mathematics — Grade 11 — Sipho Dlamini (Remote)").
        // For carried-forward lines it is prefixed "Outstanding from Oct 2026 — ".
        [StringLength(300)]
        public string? Description { get; set; }

        public BillingType BillingType { get; set; }

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public LineItemStatus Status { get; set; } = LineItemStatus.Pending;

        // For a normal line this is the billing month's due date. For a carried-forward line it
        // is the ORIGINAL due date, so receipts show which month the debt belongs to.
        public DateTime DueDate { get; set; }

        // True when this line is an unpaid balance rolled in from an earlier invoice.
        public bool IsCarriedForward { get; set; }
        public int? CarriedFromLineItemId { get; set; }

        // Exactly one extension allowed, ever, per line item. (Not used yet.)
        public DateTime? ExtensionGrantedAt { get; set; }
        public int? ExtensionGrantedByAdminUserId { get; set; }
    }
}
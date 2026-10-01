using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Models;

namespace Tutor_Manager.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        // Current Offering. Changes in place on redirect — never creates a new record.
        [Required]
        public int OfferingId { get; set; }
        public Offering Offering { get; set; } = null!;

        // Guardian-requested change awaiting Admin review (spec §10.5-10.6).
        // Valid while Status is Pending, Waitlisted, or Active. Cleared once Admin acts,
        // regardless of outcome.
        public int? RequestedOfferingId { get; set; }
        public Offering? RequestedOffering { get; set; }

        // Who filed the RequestedOfferingId change — needed so ApproveRequestedChangeAsync
        // can attribute the resulting EnrollmentRedirectHistory row to the actual Guardian,
        // not the Admin who approved it. Null when there's no pending change request.
        public int? RequestedChangeByUserId { get; set; }

        [Required]
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        [Required]
        public InitiatedByType InitiatedByType { get; set; }

        [Required]
        public int InitiatedByUserId { get; set; }

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DecidedAt { get; set; }
        public int? DecidedByAdminUserId { get; set; }
        public Administrator? DecidedByAdmin { get; set; }

        // Structured rejection — distinguishes Admin-rejection from Guardian-cancellation
        // (spec §11.5) via the code, with the note carrying human-readable detail.
        public RejectionReasonCode? RejectionReasonCode { get; set; }

        [StringLength(500)]
        public string? RejectionReasonNote { get; set; }

        // Guardian requests a withdrawal; Admin finalizes it. These two fields capture
        // the *request*; the fields below capture the *finalized* withdrawal. Mirrors the
        // same request/finalize split already used for RequestedOfferingId.
        public DateTime? WithdrawalRequestedAt { get; set; }

        [StringLength(500)]
        public string? WithdrawalRequestedReason { get; set; }

        public DateTime? WithdrawnAt { get; set; }
        public WithdrawalReasonCode? WithdrawalReasonCode { get; set; }

        [StringLength(500)]
        public string? WithdrawalReasonNote { get; set; }

        public int? WithdrawnByAdminUserId { get; set; }
        public Administrator? WithdrawnByAdmin { get; set; }

        // Set when an Active enrollment is suspended for unresolved non-payment
        // (Phase B trigger). Cleared automatically on restore to Active.
        public DateTime? SuspendedAt { get; set; }

        [Required]
        public DeliveryMode DeliveryMode { get; set; }

        // Denormalized convenience count for quick display; EnrollmentRedirectHistory
        // remains the source of truth for detail (who/when/why/previous-new Offering).
        public int RedirectCount { get; set; } = 0;

        public ICollection<EnrollmentRedirectHistory> RedirectHistory { get; set; }
            = new List<EnrollmentRedirectHistory>();

        // No IsActive flag — Status alone fully describes state (spec §15.17).
        // No hard deletes, ever (spec §15.18).
    }
}
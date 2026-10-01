using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    // Purely additive audit trail. Never creates a new Enrollment, never
    // overwrites anything on the Enrollment itself — see Section 13.
    public class EnrollmentRedirectHistory
    {
        [Key]
        public int RedirectHistoryId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        [Required]
        public int PreviousOfferingId { get; set; }
        public Offering PreviousOffering { get; set; } = null!;

        [Required]
        public int NewOfferingId { get; set; }
        public Offering NewOffering { get; set; } = null!;

        [Required]
        public DateTime RedirectedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        public InitiatedByType InitiatedByType { get; set; }

        [Required]
        public int InitiatedByUserId { get; set; }
    }
}
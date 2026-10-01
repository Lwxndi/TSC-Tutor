using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorUnavailability
    {
        public int TutorUnavailabilityId { get; set; }

        [ForeignKey("Tutor")]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [StringLength(255)]
        public string? Reason { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class Tutor
    {
        
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [StringLength(255)]
        public string? Qualification { get; set; }

        [StringLength(2000)]
        public string? Bio { get; set; }

        [Required]
        [RegularExpression("Pending|Approved|Rejected",
            ErrorMessage = "Vetting status must be Pending, Approved, or Rejected.")]
        public string VettingStatus { get; set; } = "Pending";

        public DateTime? DateApproved { get; set; }



     
        [Required]
        public AccountStatus AccountStatus { get; set; } = AccountStatus.NotCreated;

        // Operational identifier, parallel to Learner's TSCxxxxxxxx pattern.
        // Generated at approval, when the Tutor record is created (not at activation).
        [StringLength(20)]
        public string? TutorNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public ICollection<TutorSubject> SubjectsTaught { get; set; } = new List<TutorSubject>();
        public ICollection<TutorAvailability> Availability { get; set; } = new List<TutorAvailability>();
        public ICollection<TutorUnavailability> Unavailability { get; set; } = new List<TutorUnavailability>();


    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public ICollection<TutorSubject> SubjectsTaught { get; set; } = new List<TutorSubject>();
    }
}

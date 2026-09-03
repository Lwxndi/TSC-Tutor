using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class TutorApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        // Format: TSC-TA-{Year}-{00001}. Separate yearly sequence, not derived
        // from ApplicationId. Uniqueness enforced via index in OnModelCreating.
        [Required]
        [StringLength(20)]
        public required string ReferenceNumber { get; set; }

        // --- Section A: Personal Information ---
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50)]
        public required string Surname { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public required string Phone { get; set; }

        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? AltPhone { get; set; }

        [Required]
        public LocationPreference LocationPreference { get; set; }

        [Required(ErrorMessage = "Area/city is required.")]
        [StringLength(100)]
        public required string AreaCity { get; set; }

        // --- Section B: Academic Information ---
      

        [StringLength(1000)]
        public string? Achievements { get; set; }

        // --- Section E: Strengths ---
        // Comma-delimited list, matching the design's "no lookup/weighting" call.
        [StringLength(500)]
        public string? StrengthsSelected { get; set; }

        [StringLength(2000)]
        public string? StrengthsNote { get; set; }

        // --- Stage 10: POPIA Consent ---
        [Required]
        public bool ConsentGiven { get; set; } = false;

        public DateTime? ConsentDate { get; set; }

        [StringLength(10)]
        public string? ConsentVersion { get; set; }

        // --- Status & Audit ---
        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

        public DateTime? DateApplied { get; set; }
        public DateTime? DateReviewed { get; set; }

        [ForeignKey("ReviewedByAdmin")]
        public int? ReviewedByAdminId { get; set; }
        public User? ReviewedByAdmin { get; set; }
        public DateTime DateCreated { get; set; }

        // Set only on approval. References Tutor.UserId (Tutor's PK).
        [ForeignKey("CreatedTutor")]
        public int? CreatedTutorId { get; set; }
        public Tutor? CreatedTutor { get; set; }

        // --- Children ---
        public ICollection<TutorApplicationSubject> Subjects { get; set; } = new List<TutorApplicationSubject>();
        public ICollection<TutorApplicationExperience> Experience { get; set; } = new List<TutorApplicationExperience>();
        public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
        public ICollection<TutorApplicationQualification> Qualifications { get; set; } = new List<TutorApplicationQualification>();
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorApplicationExperience
    {
        [Key]
        public int ExperienceId { get; set; }

        [ForeignKey("TutorApplication")]
        public int ApplicationId { get; set; }
        public TutorApplication TutorApplication { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public required string Institution { get; set; }

        // Free text — this is the applicant's own past employer's subject
        // naming, deliberately not FK'd to the Subjects table.
        [StringLength(300)]
        public string? SubjectsTaught { get; set; }

        [StringLength(50)]
        public string? GradeLevels { get; set; }

        [StringLength(50)]
        public string? Duration { get; set; }

        [StringLength(1000)]
        public string? Responsibilities { get; set; }

        [StringLength(1000)]
        public string? Achievements { get; set; }
    }
}
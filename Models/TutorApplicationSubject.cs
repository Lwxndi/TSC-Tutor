using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorApplicationSubject
    {
        [Key]
        public int TutorApplicationSubjectId { get; set; }

        [ForeignKey("TutorApplication")]
        public int ApplicationId { get; set; }
        public TutorApplication TutorApplication { get; set; } = null!;

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        // Comma-delimited, e.g. "10,11,12" — validated (at least one) in the
        // ViewModel/controller, not here.
        [Required(ErrorMessage = "Select at least one grade level.")]
        [StringLength(20)]
        public required string GradeLevels { get; set; }

        [StringLength(50)]
        public string? CompetencyNote { get; set; }

        // Free-text note only — not a formal field competing with the transcript.
        [StringLength(500)]
        public string? ResultNote { get; set; }
    }
}
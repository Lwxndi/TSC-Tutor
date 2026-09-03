// Models/TutorApplicationQualification.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorApplicationQualification
    {
        [Key]
        public int QualificationId { get; set; }

        [ForeignKey("TutorApplication")]
        public int ApplicationId { get; set; }
        public TutorApplication TutorApplication { get; set; } = null!;

        [Required(ErrorMessage = "Select a qualification type.")]
        [StringLength(50)]
        public required string QualificationType { get; set; }

        [StringLength(150)]
        public string? Institution { get; set; }

        [StringLength(100)]
        public string? FieldOfStudy { get; set; }

        public int? YearCompleted { get; set; }

        [Required(ErrorMessage = "Select a status.")]
        [StringLength(50)]
        public required string StudyStatus { get; set; }
    }
}
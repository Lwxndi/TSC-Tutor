using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationExperienceEntryViewModel
    {
        [Required(ErrorMessage = "Institution/organisation is required.")]
        [StringLength(150)]
        public string Institution { get; set; } = string.Empty;

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
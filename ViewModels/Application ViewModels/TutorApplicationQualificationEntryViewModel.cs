// TutorApplicationQualificationEntryViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationQualificationEntryViewModel
    {
        [Required(ErrorMessage = "Select a qualification type.")]
        public string QualificationType { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Institution { get; set; }

        [StringLength(100)]
        public string? FieldOfStudy { get; set; }

        public int? YearCompleted { get; set; }

        [Required(ErrorMessage = "Select a status.")]
        public string StudyStatus { get; set; } = string.Empty;
    }
}
// TutorApplicationAcademicInfoViewModel.cs — replaced
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationAcademicInfoViewModel : IValidatableObject
    {
        [Required]
        public int ApplicationId { get; set; }

        public List<TutorApplicationQualificationEntryViewModel> Qualifications { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Qualifications.Any())
            {
                yield return new ValidationResult(
                    "Add at least one qualification.",
                    new[] { nameof(Qualifications) });
            }
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationExperienceViewModel : IValidatableObject
    {
        [Required]
        public int ApplicationId { get; set; }

        public bool HasExperience { get; set; }

        public List<TutorApplicationExperienceEntryViewModel> Entries { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Only blocks when the applicant says "Yes" but adds nothing —
            // "No" skips this section with zero blockers, per Stage 6.
            if (HasExperience && !Entries.Any())
            {
                yield return new ValidationResult(
                    "Add at least one entry, or switch back to \"No previous experience.\"",
                    new[] { nameof(Entries) });
            }
        }
    }
}
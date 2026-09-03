using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Tutor_Manager.ViewModels.TutorApplication
{
    public enum ApplicationDecision
    {
        Approve,
        Reject,
        RequestChanges
    }

    public class AdminApplicationDecisionViewModel : IValidatableObject
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Select a decision.")]
        public ApplicationDecision Decision { get; set; }

        [StringLength(1000)]
        public string? ChangesRequiredNotes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Decision == ApplicationDecision.RequestChanges &&
                string.IsNullOrWhiteSpace(ChangesRequiredNotes))
            {
                yield return new ValidationResult(
                    "Explain what needs to change before requesting changes.",
                    new[] { nameof(ChangesRequiredNotes) });
            }
        }
    }
}
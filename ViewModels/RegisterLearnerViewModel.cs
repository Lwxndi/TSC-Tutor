using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    public class RegisterLearnerViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public required string PhoneNumber { get; set; }

        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? AltPhoneNumber { get; set; }

        public Gender? Gender { get; set; }

        // THE RAW password (never stored). Strength rules live here, not on
        // User.PasswordHash - the hash is generated from this after validation passes.
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&^#()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }

        // Required - decided during design that a Learner must have a grade to register.
        [Required(ErrorMessage = "Please select your grade.")]
        public Grade? GradeLevel { get; set; }

        [StringLength(100)]
        public string? SchoolName { get; set; }

        // Populated by the controller (GET) from the Subjects table for checkbox
        // rendering; comes back with IsSelected set correctly on POST.
        public List<SubjectSelection> AvailableSubjects { get; set; } = new();

        // Links up to 3 guardians at registration time, by phone number (guardians
        // typically communicate by phone/WhatsApp, not email). Whether/how these
        // become User accounts if none exists yet is a controller-level decision.
        [Required(ErrorMessage = "At least one guardian phone number is required.")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public required string GuardianPhoneNumber1 { get; set; }

        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? GuardianPhoneNumber2 { get; set; }

        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? GuardianPhoneNumber3 { get; set; }

        [StringLength(50)]
        public string? GuardianRelationship { get; set; } // "Mother", "Father", "Guardian"...

        // Cross-field rule that a single attribute can't express: at least one
        // subject must be checked - registering with zero subjects isn't meaningful
        // for a tutoring platform.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvailableSubjects is null || !AvailableSubjects.Any(s => s.IsSelected))
            {
                yield return new ValidationResult(
                    "Please select at least one subject.",
                    new[] { nameof(AvailableSubjects) });
            }
        }
    }
}
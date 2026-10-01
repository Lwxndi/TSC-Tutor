using System.ComponentModel.DataAnnotations;


namespace Tutor_Manager.ViewModels
{
    public class RegisterGuardianViewModel
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

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Enter your child's TSC number.")]
        [StringLength(20)]
        public required string LearnerTscNumber { get; set; }

        [StringLength(50)]
        public string? RelationshipToLearner { get; set; } // "Mother", "Father", "Guardian"...
    }
}
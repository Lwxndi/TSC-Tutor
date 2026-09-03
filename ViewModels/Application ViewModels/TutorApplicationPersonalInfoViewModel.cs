using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationPersonalInfoViewModel
    {
        public int? ApplicationId { get; set; }

        // Display-only once the record exists; not posted back.
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50)]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? AltPhone { get; set; }

        [Required(ErrorMessage = "Please select a location preference.")]
        public LocationPreference LocationPreference { get; set; }

        [Required(ErrorMessage = "Area/city is required.")]
        [StringLength(100)]
        public string AreaCity { get; set; } = string.Empty;
    }
}
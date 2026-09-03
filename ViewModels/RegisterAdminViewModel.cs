// ViewModels/RegisterAdminViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class RegisterAdminViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50)]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Position { get; set; }
    }
}